using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traktor;
using System;
using System.IO;
using System.Text;
using Traktor.Propagation;
using OpenTracing;

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
            Format binaryformat = new Format();
            Binary binaryCarrier = new Binary();
            MemoryStream memstrm = new MemoryStream();
           // ASCIIEncoding encoding = new ASCIIEncoding();
           // byte[] contextString = encoding.GetBytes(serverscope.Span.Context.ToString());
           // memstrm.Write(contextString);
            binaryCarrier.Set(memstrm);
            serverTracer.Inject<Binary>(serverscope.Span.Context, binaryformat, binaryCarrier);
            IScope clientscope = clientTracer.BuildSpan("Client-Operation").AsChildOf(clientTracer.Extract<Binary>(binaryformat, binaryCarrier)).StartActive();
            clientscope.Span.Finish();
            serverscope.Span.Finish();

            Console.WriteLine(clientscope.Span.ToString());
            Console.WriteLine(serverscope.Span.ToString());


        }
    }
}
