using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traktor;
using OpenTracing;

namespace Traktor_Test
{
    [TestClass]
    public class SpanBuilderTests
    {
        [TestMethod]
        public void Start() 
        {
            string expectedOperationName = "Testoperation";
            Tracer tracer = new Tracer();
            ISpanBuilder builder = tracer.BuildSpan(expectedOperationName);
            ISpan span = builder.Start();
            string[] actualSpanFields = span.ToString().Split(";");
            string actualOperationName = actualSpanFields[0];
            string actualStartTimeStamp = actualSpanFields[1];
            string actualSpanContext = actualSpanFields[2];

            Assert.AreEqual(expectedOperationName, actualOperationName);
            Assert.IsNotNull(actualStartTimeStamp);
            
        }
    }
}
