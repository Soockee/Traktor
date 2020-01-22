using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Traktor;
using OpenTracing;
namespace Traktor_Test
{
    [TestClass]
    public class ReporterTests
    {
        [TestMethod]
        public void Report_WithValidSpan()
        {
            string expectedOperationName = "Operationname-4Head";
            string address = "localhost";
            int agentport = 13337;
            int reporterport = 13338;
            // Setup Recieving Endpoint before Tracer.Reporter Connects to this Client and before Span Finish() is Called
            Task agentTast = Task.Run(() => {
                
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                UdpClient agent = new UdpClient(agentport);
                Byte[] recieveBytes = agent.Receive(ref RemoteIpEndPoint);
                string spanContent = Encoding.UTF8.GetString(recieveBytes);
                // Uses the IPEndPoint object to determine which of these two hosts responded.
                // Output can be used to comprehend the communication
                Console.WriteLine("This is the message you received " +
                                             spanContent.ToString());
                Console.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());
                Assert.AreEqual(expectedOperationName, spanContent.ToString().Split(";")[0]);
                Assert.IsNotNull(DateTime.Parse(spanContent.ToString().Split(";")[5]));
            });
            Tracer tracer = new Tracer();
            //Cautions: Test run parallel ->  Portusage needs to vary between tests
            tracer.Configure(address, agentport, reporterport);
            IScope scope = tracer.BuildSpan(expectedOperationName).StartActive();
            ISpan span = scope.Span;
            span.Finish();
            agentTast.Wait();
            Assert.AreNotEqual(DateTime.MinValue, DateTime.Parse(span.ToString().Split(";")[5]));
        }
        [TestMethod]
        public void Report_ValidSpanWithReference()
        {
            string expectedOperationName = "Operationname-4Head";
            string address = "localhost";
            int agentport = 13348;
            int reporterport = 13347;
            // Setup Recieving Endpoint before Tracer.Reporter Connects to this Client and before Span Finish() is Called
            Task agentTast = Task.Run(() => {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                UdpClient agent = new UdpClient(agentport);
                for(int i = 0; i < 2; ++i)
                {
                    Byte[] recieveBytes = agent.Receive(ref RemoteIpEndPoint);
                    string spanContent = Encoding.UTF8.GetString(recieveBytes);
                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    // Output can be used to comprehend the communication
                    Console.WriteLine("This is the message you received " +
                                                 spanContent.ToString());
                    Console.WriteLine("This message was sent from " +
                                                RemoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                RemoteIpEndPoint.Port.ToString());
                    Assert.AreEqual(expectedOperationName, spanContent.ToString().Split(";")[0]);
                }
                //Assert.IsNotNull(DateTime.Parse(spanContent.ToString().Split(";")[5]));
            });
            Tracer tracer = new Tracer();
            //Cautions: Test run parallel ->  Portusage needs to vary between tests
            tracer.Configure(address, agentport, reporterport);
            IScope scope = tracer.BuildSpan(expectedOperationName).StartActive();
            IScope scope2 = tracer.BuildSpan(expectedOperationName).StartActive();
            scope2.Span.Finish();
            scope.Span.Finish();

            agentTast.Wait();      
        }
        [TestMethod]
        [Ignore]
        public void Report_LargeSpan()
        {
            Tracer tracer = new Tracer();
            string address = "127.0.0.1";
            int port = 8080;
            int agentport = 13337;

            tracer.Configure(address, agentport, port);     
            ISpanContext context = new SpanContext("123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123", "abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc");
            var span = tracer.BuildSpan("opnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTESTopnameTEST").AsChildOf(context).Start();
            tracer.ScopeManager.Activate(span,true);
            span.Finish();
        }
    }
}
