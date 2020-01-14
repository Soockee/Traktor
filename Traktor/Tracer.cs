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
        private IScopeManager scopemanager;
        public IScopeManager ScopeManager { get { return scopemanager; } }
        public ISpan ActiveSpan => scopemanager.Active?.Span;
        public Reporter reporter;

        public Tracer()
        {
            scopemanager = new AsyncLocalScopeManager();

        }

        // Configuration Class might be appropiate
        // for now all Config as Parameter
        public void Configure()
        {
            
        }
        public void Configure(string agentaddress, int agentport, int reporterport) 
        {
            reporter = new Reporter(agentaddress, agentport, reporterport,this);
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
