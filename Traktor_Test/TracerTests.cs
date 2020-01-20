using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTracing;
using OpenTracing.Propagation;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traktor;
using Traktor.Propagation;

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
            string address = "127.0.0.1";
            string port = "8080";
            tracer.Configure(address,port);
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
        public void Broadcast_Context()
        {
            Tracer tracer = new Tracer();
            string address = "127.0.0.1";
            string port = "8080";
            tracer.Configure(address, port);

            Tracer tracer2 = new Tracer();
            tracer2.Configure(address, port);

            BinaryCarrier carrier_beginning = new BinaryCarrier();
            var scope = tracer.BuildSpan("kek").StartActive();
            tracer.Inject(scope.Span.Context, BuiltinFormats.Binary, carrier_beginning);

            tracer.registry.SendAsync(carrier_beginning.Get().ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);


            var buffer = new byte[4096 * 20];
            BinaryCarrier carrier_end = new BinaryCarrier();
            WebSocketReceiveResult result = tracer2.registry.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            if (result.Count != 0 || result.CloseStatus == WebSocketCloseStatus.Empty)
            {
                carrier_end.Set(new MemoryStream(buffer));
                ISpanContext context = tracer2.Extract(BuiltinFormats.Binary, carrier_end);
                Console.WriteLine(context.ToString());
            }
            /*
            byte[] array2 = Encoding.UTF8.GetBytes("context2");
            tracer2.registry.SendAsync(array2, WebSocketMessageType.Binary, true, CancellationToken.None);


            var buffer2 = new byte[4096 * 20];

            WebSocketReceiveResult result2 = tracer.registry.ReceiveAsync(new ArraySegment<byte>(buffer2), CancellationToken.None).Result;   
            if (result2.Count != 0 || result2.CloseStatus == WebSocketCloseStatus.Empty)
            {
                string message = Encoding.UTF8.GetString(buffer2, 0, result2.Count);
                Console.WriteLine(message);
            }

            var buffer3 = new byte[4096 * 20];
            WebSocketReceiveResult result3 = tracer.registry.ReceiveAsync(new ArraySegment<byte>(buffer3), CancellationToken.None).Result;
            if (result3.Count != 0 || result3.CloseStatus == WebSocketCloseStatus.Empty)
            {
                string message = Encoding.UTF8.GetString(buffer3, 0, result3.Count);
                Console.WriteLine(message);
            }
            */
            tracer.Dispose();
            tracer2.Dispose();
        }


        [TestMethod]
        [Ignore]
        public void  Broadcast_strings()
        {
            Tracer tracer = new Tracer();
            string address = "127.0.0.1";
            string port = "8080";
            tracer.Configure(address, port);

            Tracer tracer2 = new Tracer();
            tracer2.Configure(address, port);

            string content = "SomeMessage-1";
            byte[] array = Encoding.UTF8.GetBytes(content);
            var buffer = new byte[4096 * 20];
            var _ClientBuffer = new ArraySegment<byte>(buffer);
            tracer.registry.SendAsync(array, WebSocketMessageType.Binary, true, CancellationToken.None);
            Task<WebSocketReceiveResult> t = tracer2.registry.ReceiveAsync(_ClientBuffer, CancellationToken.None);
            t.Wait();
            WebSocketReceiveResult result = t.Result;
            if(result.Count != 0 ||result.CloseStatus == WebSocketCloseStatus.Empty)
            {
                string message = Encoding.UTF8.GetString(_ClientBuffer.Array,
             _ClientBuffer.Offset, result.Count);
                Console.WriteLine(message);
            }

            string afterPrintMessage = "after";
            byte[] array2 = Encoding.UTF8.GetBytes(afterPrintMessage);
            var buffer2 = new byte[4096 * 20];
            var _ClientBuffer2 = new ArraySegment<byte>(buffer2);


            tracer2.registry.SendAsync(array2, WebSocketMessageType.Binary, true, CancellationToken.None);

            Task<WebSocketReceiveResult> t2 = tracer.registry.ReceiveAsync(_ClientBuffer2, CancellationToken.None);
            t2.Wait();
            WebSocketReceiveResult result2 = t2.Result;
            if (result2.Count != 0 || result2.CloseStatus == WebSocketCloseStatus.Empty)
            {
                string message = Encoding.UTF8.GetString(_ClientBuffer2.Array,
             _ClientBuffer2.Offset, result2.Count);
                Console.WriteLine(message);
            }

            var buffer3 = new byte[4096 * 20];
            WebSocketReceiveResult result3 = tracer.registry.ReceiveAsync(new ArraySegment<byte>(buffer3), CancellationToken.None).Result;
            if (result3.Count != 0 || result3.CloseStatus == WebSocketCloseStatus.Empty)
            {
                string message = Encoding.UTF8.GetString(buffer3, 0, result3.Count);
                Console.WriteLine(message);
            }
            tracer.Dispose();
            tracer2.Dispose();
        }
    }
}
