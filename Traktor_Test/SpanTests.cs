using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using Traktor;
using OpenTracing;
namespace Traktor_Test
{
    [TestClass]
    public class SpanTests
    {
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), "Span.toString() doesn't contain an endtime of an unfinished Span")]
        public void Check_Endtime_With_Unfinished_Span()
        {
            string expectedOperationName = "Operationname-4Head";
            Tracer tracer = new Tracer();
            IScope scope = tracer.BuildSpan(expectedOperationName).StartActive();
            ISpan span = scope.Span;
            string s = span.ToString().Split(";")[5];
            Assert.IsNull(s);
        }
        [TestMethod]
        public void Finish() 
        {
            /*[0] = Operationname
             *[1] = Starttime  
             *[2] = TraceId
             *[3] = SpanId
             *[4] = Reference_type
             *[5] = Endtime
             */
            //Configure needed to handle dispose and finish of spans
            //Cautions: Test run parallel ->  Portusage needs to vary between tests
            string address = "localhost";
            int agentport = 13341;
            int reporterport = 13342;
            string expectedOperationName = "operationname";
            Tracer tracer = new Tracer();
            tracer.Configure(address, agentport, reporterport);

            ISpan span = tracer.BuildSpan("operationname").Start();

            span.Finish();
            Assert.IsNull(tracer.ActiveSpan);
            using( var scope = tracer.BuildSpan("somename").StartActive())
            {
                Assert.IsNotNull(tracer.ActiveSpan);

            }
            Assert.IsNull(tracer.ActiveSpan);
            Assert.AreNotEqual(DateTime.MinValue, DateTime.Parse(span.ToString().Split(";")[5]));
            string[] actualSpanFields = span.ToString().Split(";");
            string actualOperationName = actualSpanFields[0];
            string startTimeSet = actualSpanFields[1];
            string actualSpanContextTraceId = actualSpanFields[2];
            string actualSpanContextSpanId = actualSpanFields[3];
            string actualSpanContextReferenceTyp = actualSpanFields[4];
            string endTimeSet = actualSpanFields[5];
            Assert.AreEqual(expectedOperationName, actualOperationName);
            // toDO: Method for IDValidation
            Assert.IsNotNull(actualSpanContextTraceId);
            Assert.IsNotNull(actualSpanContextSpanId);
            Assert.AreEqual(References.ChildOf, actualSpanContextReferenceTyp);
            Assert.IsNotNull(startTimeSet);
            Assert.IsNotNull(endTimeSet);
            Assert.IsNotNull(DateTime.Parse(startTimeSet));
            Assert.IsNotNull(DateTime.Parse(endTimeSet));
        }
    }
}
