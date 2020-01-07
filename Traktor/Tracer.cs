using System;
using Traktor;
using Traktor.Propagation;


using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
namespace Traktor
{
    public sealed class Tracer: ITracer
    {
        internal static readonly Tracer Instance = new Tracer();

        private IScopeManager scopemanager;

        public IScopeManager ScopeManager { get { return scopemanager; } }
        public ISpan ActiveSpan => scopemanager.Active?.Span;

        public Tracer()
        {
            scopemanager = new AsyncLocalScopeManager();
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
