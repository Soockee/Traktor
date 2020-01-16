using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traktor;
using OpenTracing;
using System.Threading.Tasks;
using System;
namespace Traktor_Test
{
    [TestClass]
    public class ScopeManagerTests
    {
        public static string Scope_with_diffrent_thread_context__parentSpanID;
        [TestMethod]
        public void Activate()
        {
            Tracer tracer = new Tracer();
            ISpan span = tracer.BuildSpan("testname").Start();
            tracer.ScopeManager.Activate(span, false);


            Assert.AreEqual(tracer.ActiveSpan, tracer.ScopeManager.Active.Span);
            Assert.AreEqual(span, tracer.ScopeManager.Active.Span);
        }
        [TestMethod]
        public void Scope_with_diffrent_thread_context()
        {
            /*[0] = Operationname
             *[1] = Starttime  
             *[2] = TraceId
             *[3] = SpanId
             *[4] = Reference_type
             *[5] = Endtime
             */
            Tracer tracer = new Tracer();
            string address = "localhost";
            int agentport = 13343;
            int reporterport = 13344;
            tracer.Configure(address, agentport, reporterport);
            using (var scope = tracer.BuildSpan("first_op_name").StartActive())
            {
                Console.WriteLine(scope.Span.ToString());
                ScopeManagerTests.Scope_with_diffrent_thread_context__parentSpanID = scope.Span.ToString().Split(";")[3];
                Task contextChanger = Task.Run(() => {
                    using(var scope = tracer.BuildSpan("second_op_name").StartActive()) 
                    {   
                        Console.WriteLine("Inside Thread");
                        Console.WriteLine(scope.Span.ToString());
                        string[] con2 = scope.Span.ToString().Split(";");
                        Assert.IsTrue(con2[4].Contains(ScopeManagerTests.Scope_with_diffrent_thread_context__parentSpanID));
                    }
                });
                using (var scope2 = tracer.BuildSpan("third_op_name").StartActive())
                {
                    Assert.IsTrue(scope2.Span.ToString().Split(";")[4].Contains(ScopeManagerTests.Scope_with_diffrent_thread_context__parentSpanID));

                    Console.WriteLine(scope2.Span.ToString());
                    Console.WriteLine("Outside Thread");
                    
                }
            }
        }
    }
}
