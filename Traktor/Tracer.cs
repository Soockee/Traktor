using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Traktor;
using Traktor.Propagation;
 

using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
using System.Net.WebSockets;
namespace Traktor
{
    public sealed class Tracer: ITracer
    {
        private IScopeManager scopemanager;
        public IScopeManager ScopeManager { get { return scopemanager; } }
        public ISpan ActiveSpan => scopemanager.Active?.Span;
        public Reporter reporter;
        public WebSocket registry;

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
            reporter = new Reporter(agentaddress, agentport, reporterport,this);
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(operationName, this);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            //hack to get carrier into BinaryCarrier
            Inject(spanContext,(BinaryCarrier)(object)carrier);
        }
        private void Inject(ISpanContext spanContext, BinaryCarrier carrier)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
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
        public ISpanContext Extract( BinaryCarrier carrier)
        {

            string[] contextvars = carrier.toString().Split(';');
            SpanContext context = new SpanContext(contextvars[0], contextvars[1],contextvars[2]);
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
        public void Register() 
        {
            var client = new ClientWebSocket();
            client.ConnectAsync(new Uri("ws://localhost:44340/ws"), CancellationToken.None);
            Console.WriteLine("Connected!");

        }  
    }
}
