using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traktor;
using OpenTracing;
namespace Traktor_Test
{
    [TestClass]
    public class ScopeManagerTests
    {
        [TestMethod]
        public void Activate_() 
        {
            ITracer tracer = new Tracer();
            ISpan span = tracer.BuildSpan("testname").Start();
            tracer.ScopeManager.Activate(span,false);


            Assert.AreEqual(tracer.ActiveSpan, tracer.ScopeManager.Active.Span);
            Assert.AreEqual(span, tracer.ScopeManager.Active.Span);
        }
    }
}
