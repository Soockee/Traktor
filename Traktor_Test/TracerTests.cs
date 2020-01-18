using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traktor;
using System;
using System.IO;
using System.Text;
using Traktor.Propagation;
using Traktor;
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
    }
}
