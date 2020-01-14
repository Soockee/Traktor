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
        [TestMethod]
        public void StartActive()
        {
            string expectedOperationName = "Testoperation";
            Tracer tracer = new Tracer();
            ISpanBuilder builder = tracer.BuildSpan(expectedOperationName);
            IScope scope = builder.StartActive();
            string[] actualSpanFields = scope.Span.ToString().Split(";");

            Assert.AreEqual(scope.Span, tracer.ActiveSpan);
            Assert.AreEqual(scope.Span, tracer.ScopeManager.Active.Span);
            Assert.AreEqual(tracer.ActiveSpan, tracer.ScopeManager.Active.Span);
        }
        //toDo: Check if Dispose is correctly handled
        [TestMethod]
        public void StartActiveWithBool()
        {
            string expectedOperationName = "Testoperation";
            bool finishSpanAfterDispose = false;
            Tracer tracer = new Tracer();
            ISpanBuilder builder = tracer.BuildSpan(expectedOperationName);
            IScope scope = builder.StartActive(finishSpanAfterDispose);
            string[] actualSpanFields = scope.Span.ToString().Split(";");

            Assert.AreEqual(scope.Span, tracer.ActiveSpan);
            Assert.AreEqual(scope.Span, tracer.ScopeManager.Active.Span);
            Assert.AreEqual(tracer.ActiveSpan, tracer.ScopeManager.Active.Span);
        }
        [TestMethod]
        public void AddReference()
        {
            string expectedOperationName_1 = "Testoperation_1";
            string expectedOperationName_2 = "Testoperation_2";
            string expectedOperationName_3 = "Testoperation_3";
            string expectedOperationName_4 = "Testoperation_4";
            //Configure needed to handle dispose and finish of spans
            //Cautions: Test run parallel ->  Portusage needs to vary between tests
            string address = "localhost";
            int agentport = 13339;
            int reporterport = 13340;

            Tracer tracer = new Tracer();
            tracer.Configure(address, agentport, reporterport);
            IScope first_scope;
            using (IScope scope = tracer.BuildSpan(expectedOperationName_1).StartActive()) {
                first_scope = scope;
                Assert.AreEqual(scope.Span, tracer.ActiveSpan);
            };
            using (IScope scope = tracer.BuildSpan(expectedOperationName_2).StartActive())
            {
                Assert.AreEqual(scope.Span, tracer.ActiveSpan);
                Assert.AreNotEqual(scope.Span, first_scope);
            };
            using (IScope scope = tracer.BuildSpan(expectedOperationName_3).StartActive())
            {
                ISpanBuilder builder = tracer.BuildSpan(expectedOperationName_4);
                using (IScope childScope = builder.StartActive())
                {
                    Assert.AreEqual(childScope.Span, tracer.ActiveSpan);
                    Assert.AreNotEqual(scope.Span, tracer.ActiveSpan);
                    Assert.AreNotEqual(scope.Span, childScope.Span);
                    Assert.IsTrue(tracer.ActiveSpan.Context.SpanId==childScope.Span.Context.SpanId);
                    Assert.IsTrue(tracer.ActiveSpan.Context.TraceId == childScope.Span.Context.TraceId);
                    Assert.IsTrue(tracer.ActiveSpan.Context.TraceId == scope.Span.Context.TraceId);
                    Assert.IsFalse(tracer.ActiveSpan.Context.SpanId == scope.Span.Context.SpanId);
                };
            };
        }
        [TestMethod]
        [Ignore]
        public void AsChildOf()
        {
            string expectedOperationName_1 = "Testoperation_1";
            string expectedOperationName_2 = "Testoperation_2";
            Tracer tracer = new Tracer();
            using (IScope scope = tracer.BuildSpan(expectedOperationName_1).StartActive())
            {
                using (IScope childScope = tracer.BuildSpan(expectedOperationName_2).AsChildOf(scope.Span.Context).StartActive())
                {
                    Assert.AreEqual(childScope.Span, tracer.ActiveSpan);
                    Assert.AreNotEqual(scope.Span, tracer.ActiveSpan);
                    Assert.AreNotEqual(scope.Span, childScope.Span);
                    Assert.IsTrue(tracer.ActiveSpan.Context.SpanId == childScope.Span.Context.SpanId);
                    Assert.IsTrue(tracer.ActiveSpan.Context.TraceId == childScope.Span.Context.TraceId);
                    Assert.IsTrue(tracer.ActiveSpan.Context.TraceId == scope.Span.Context.TraceId);
                    Assert.IsFalse(tracer.ActiveSpan.Context.SpanId == scope.Span.Context.SpanId);
                };
            };

        }
    }
}
