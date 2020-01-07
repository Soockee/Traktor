using System;
using Traktor;
using Traktor.Propagation;


using OpenTracing;
using OpenTracing.Propagation;
namespace Traktor
{
    public sealed class Tracer: ITracer
    {
        internal static readonly Tracer Instance = new Tracer();

        private ScopeManager scopemanager;

        public IScopeManager ScopeManager { get { return scopemanager; } }
        public ISpan ActiveSpan => scopemanager.Active?.Span;

        public Tracer()
        {
            scopemanager = new ScopeManager();
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(operationName, this);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return SpanContext.Instance;
        }

        public override string ToString()
        {
            return nameof(Tracer);
        }
    }
}
