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
        public void StartActive()
        {
            string expectedOperationName = "Testoperation";
            Tracer tracer = new Tracer();
            ISpanBuilder builder = tracer.BuildSpan(expectedOperationName);
            IScope scope = builder.StartActive();
            string[] actualSpanFields = scope.Span.ToString().Split(";");

            Assert.AreEqual(scope.Span, tracer.ActiveSpan);
            Assert.AreEqual(scope.Span, tracer.ScopeManager.Active);
            Assert.AreEqual(tracer.ActiveSpan, tracer.ScopeManager.Active);
        }
        //toDo: Check if Dispose is correctly handled
        public void StartActiveWithBool()
        {
            string expectedOperationName = "Testoperation";
            bool finishSpanAfterDispose = false;
            Tracer tracer = new Tracer();
            ISpanBuilder builder = tracer.BuildSpan(expectedOperationName);
            IScope scope = builder.StartActive(finishSpanAfterDispose);
            string[] actualSpanFields = scope.Span.ToString().Split(";");

            Assert.AreEqual(scope.Span, tracer.ActiveSpan);
            Assert.AreEqual(scope.Span, tracer.ScopeManager.Active);
            Assert.AreEqual(tracer.ActiveSpan, tracer.ScopeManager.Active);
        }
        public void AddReference()
        {
           
        }

    }
}
