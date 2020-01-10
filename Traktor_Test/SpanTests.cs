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
        public void Finish() 
        {
            string expectedOperationName = "operationname";
            Tracer tracer = new Tracer();
            ISpan span = tracer.BuildSpan("operationname").Start();
            span.Finish();
            string[] actualSpanFields = span.ToString().Split(";");
            string actualOperationName = actualSpanFields[0];
            string actualSpanContext = actualSpanFields[2];

            string startTimeSet = actualSpanFields[3];
            string endTimeSet = actualSpanFields[3];

            Assert.AreEqual(expectedOperationName, actualOperationName);
            Assert.IsNotNull(startTimeSet);
            Assert.IsNotNull(endTimeSet);
            Assert.IsNotNull(DateTime.Parse(startTimeSet));
            Assert.IsNotNull(DateTime.Parse(endTimeSet));


        }
    }
}
