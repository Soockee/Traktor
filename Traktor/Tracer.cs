﻿using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traktor.Propagation;
namespace Traktor
{
    public sealed class Tracer : ITracer, IDisposable
    {
        private IScopeManager scopemanager;
        public IScopeManager ScopeManager { get { return scopemanager; } }
        public ISpan ActiveSpan => scopemanager.Active?.Span;
        public Reporter reporter;
        public ClientWebSocket registry;

        public Tracer()
        {
            scopemanager = new AsyncLocalScopeManager();

        }

        // Configuration Class might be appropiate
        // for now all Config as Parameter
        public void Configure()
        {

        }
        public void Configure(string agentaddress, int agentport, int reporterport)
        {
            reporter = new Reporter(agentaddress, agentport, reporterport, this);
        }
        public void Configure(string registryadress, string registryport, string agentaddress, int agentport, int reporterport)
        {
            reporter = new Reporter(agentaddress, agentport, reporterport, this);
            registry = Register(registryadress, registryport).GetAwaiter().GetResult();

        }
        public void Configure(string registryadress, string registryport)
        {
            registry = Register(registryadress, registryport).GetAwaiter().GetResult();
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(operationName, this);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            //hack to get carrier into BinaryCarrier
            Inject(spanContext, (BinaryCarrier)(object)carrier);
        }
        private void Inject(ISpanContext spanContext, BinaryCarrier carrier)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] contextString = encoding.GetBytes(ActiveSpan.Context.ToString());
            MemoryStream memstrm = new MemoryStream();
            memstrm.Write(contextString, 0, contextString.Length);
            carrier.Set(memstrm);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {

            //Close Carrier Memstream
            return Extract((BinaryCarrier)(object)carrier);
        }
        public ISpanContext Extract(BinaryCarrier carrier)
        {

            string[] contextvars = carrier.toString().Split(';');
            SpanContext context = new SpanContext(contextvars[0], contextvars[1], contextvars[2]);
            return (ISpanContext)context;
        }

        public override string ToString()
        {
            return nameof(Tracer);
        }

        /* Starting with naive direct connection, should be something smarter.
         * Messagebroker?
         * RegistrationService?
         * 
         * 
        */
        public async Task<ClientWebSocket> Register(string url,string port)
        {
            ClientWebSocket registry = new ClientWebSocket();
            registry.Options.KeepAliveInterval = TimeSpan.Zero;
            await registry.ConnectAsync(new Uri("ws://"+url+":"+ port), CancellationToken.None);

            return registry;
        }
        public void Dispose()
        {
            CancellationToken token = new CancellationToken();
            registry.CloseAsync(WebSocketCloseStatus.NormalClosure, "Tracer-Client Disposed", token);
        }
        public async Task<ISpanContext> ReceiveContext() 
        {
            int initalBufferSize = 512;
            BinaryCarrier carrier = new BinaryCarrier();
            byte[] buffer = new byte[initalBufferSize];
            var offset = 0;
            var free = buffer.Length;
            WebSocketReceiveResult result;

            while(true)
            {
                /*
                 * Algorithm source: https://stackoverflow.com/a/41926694/738359
                 * by Matthias247 checked: 24.01.2020
                 * modified by: Simon Stockhause
                 * */
                result = await registry.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), CancellationToken.None);

                if(result.EndOfMessage)
                {
                    if(result.Count != 0 || result.CloseStatus == WebSocketCloseStatus.Empty) 
                    {
                        break;
                    }
                    else 
                    {
                        throw new ArgumentException("Received BinaryCarrier is invalid"); 
                    }
                }
                if (free == 0) 
                {
                    var newSize = buffer.Length + initalBufferSize;
                    var newBuffer = new Byte[newSize];
                    Array.Copy(buffer, 0, newBuffer,0,offset);
                    buffer = newBuffer;
                    free = buffer.Length - offset;
                }
            }
            byte[] context = new byte[result.Count];
            for(int i = 0; i< context.Length; ++i)
            {
                context[i] = buffer[i];
            }
            carrier.Set(new MemoryStream(context));
            return Extract(BuiltinFormats.Binary, carrier);
        }

        public async Task SendContext(ISpan span) 
        {
            BinaryCarrier carrier = new BinaryCarrier();
            Inject(span.Context, BuiltinFormats.Binary, carrier);
            await registry.SendAsync(new ArraySegment<byte>(carrier.Get().ToArray()), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }
}
