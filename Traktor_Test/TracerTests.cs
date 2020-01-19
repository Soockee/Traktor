using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traktor;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

using Traktor.Propagation;
using OpenTracing;
using OpenTracing.Propagation;

namespace Traktor_Test
{
    [TestClass]
    public class TracerTests
    {

        [TestMethod]
        public void Context_Send() 
        {
            Tracer serverTracer = new Tracer();
            Tracer clientTracer = new Tracer();
            string address = "localhost";
            int serverAgentport = 13343;
            int serverReporterport = 13344;
            int clientAgentport = 13345;
            int clientReporterport = 13346;
            serverTracer.Configure(address, serverAgentport, serverReporterport);
            clientTracer.Configure(address, clientAgentport, clientReporterport);

            IScope serverscope = serverTracer.BuildSpan("Server-Operation").StartActive();
            BinaryCarrier binaryCarrier = new BinaryCarrier();
            serverTracer.Inject<IBinary>(serverscope.Span.Context, BuiltinFormats.Binary, binaryCarrier);
            ISpanContext serverContext = clientTracer.Extract<IBinary>(BuiltinFormats.Binary, binaryCarrier);
            IScope clientscope = clientTracer.BuildSpan("Client-Operation").AsChildOf(serverContext).StartActive();
            clientscope.Span.Finish();
            serverscope.Span.Finish();

            Console.WriteLine(serverscope.Span.ToString());
            Console.WriteLine(clientscope.Span.ToString());
        }
        [TestMethod]
        public void byteBufferInjectionAndExtraction() 
        {
            Tracer tracer1 = new Tracer();
            Tracer tracer2 = new Tracer();
            ASCIIEncoding encoding = new ASCIIEncoding();
            BinaryCarrier carrier = new BinaryCarrier();
            var scope = tracer1.BuildSpan("kek").StartActive();       
            tracer1.Inject(scope.Span.Context, BuiltinFormats.Binary, carrier);
            ISpanContext context = tracer2.Extract(BuiltinFormats.Binary, carrier);
            Assert.AreEqual(context.ToString(), scope.Span.Context.ToString());
        }
        [TestMethod]
        [Ignore]
        public void broadcast_context_via_Registry()
        {
            Tracer tracer1 = new Tracer();
            Tracer tracer2 = new Tracer();
            ASCIIEncoding encoding = new ASCIIEncoding();
            BinaryCarrier carrier = new BinaryCarrier();
            var scope = tracer1.BuildSpan("kek").StartActive();
            tracer1.Inject(scope.Span.Context, BuiltinFormats.Binary, carrier);
            ISpanContext context = tracer2.Extract(BuiltinFormats.Binary, carrier);
            Assert.AreEqual(context.ToString(), scope.Span.Context.ToString());
        }
        [TestMethod]
        [Ignore]
        public void Register()
        {
            Tracer tracer = new Tracer();
            tracer.Configure("ws://127.0.0.1:8080");
            string content = "SomeMessage-1";
            string content1 = "SomeMessage-2";
            string content2 = "SomeMessage-3";
            byte[] array = Encoding.ASCII.GetBytes(content);
            byte[] array1 = Encoding.ASCII.GetBytes(content1);
            byte[] array2 = Encoding.ASCII.GetBytes(content2);
            CancellationToken token = new CancellationToken();
            tracer.registry.SendAsync(array, WebSocketMessageType.Binary, true, token);
            tracer.registry.SendAsync(array1, WebSocketMessageType.Binary, true, token);
            tracer.registry.SendAsync(array2, WebSocketMessageType.Binary, true, token);
        }
        [TestMethod]
        public void  Broadcast()
        {
            Tracer tracer = new Tracer();
            tracer.Configure("ws://127.0.0.1:8080");

           // Tracer tracer2 = new Tracer();
            //tracer2.Configure("ws://127.0.0.1:8080");

            string content = "SomeMessage-1";
            string afterPrintMessage = "after";
            byte[] array = Encoding.UTF8.GetBytes(content);
            byte[] array1 = Encoding.UTF8.GetBytes(afterPrintMessage);
            var buffer = new byte[4096 * 20];
            var buffer2 = new byte[4096 * 20];
            var _ClientBuffer = new ArraySegment<byte>(buffer);
            tracer.registry.SendAsync(array, WebSocketMessageType.Binary, true, CancellationToken.None);
            Task<WebSocketReceiveResult> t = tracer.registry.ReceiveAsync(_ClientBuffer, CancellationToken.None);
            t.Wait();
            WebSocketReceiveResult result = t.Result;
            if(result.Count != 0 ||result.CloseStatus == WebSocketCloseStatus.Empty)
            {
                string message = Encoding.UTF8.GetString(_ClientBuffer.Array,
             _ClientBuffer.Offset, result.Count);
                Console.WriteLine(message);
            }
            tracer.registry.SendAsync(array1, WebSocketMessageType.Binary, true, CancellationToken.None);

            tracer.Dispose();
            // tracer2.Dispose();
        }
    }
}
