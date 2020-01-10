using System;
using System.Collections.Generic;
using System.Text;

using OpenTracing;
using OpenTracing.Tag;


namespace Traktor
{
    public sealed class SpanBuilder : ISpanBuilder
    {
        //internal static ISpanBuilder Instance = new SpanBuilder("kek");

        public string operationName;
        private Tracer tracer;
        private Dictionary<string, string> references;

        public  SpanBuilder(string operationName, Tracer tracer)
        {
            this.operationName = operationName;
            this.tracer = tracer;
            this.references = new Dictionary<string, string>();
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (referencedContext == null) return this;
            references.Add(referencedContext.SpanId, referenceType);
            return this;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            AddReference(References.ChildOf, parent);
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            AddReference(References.ChildOf, parent.Context);
            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            return this;
        }

        public ISpanBuilder WithTag(BooleanTag tag, bool value)
        {
            return this;
        }

        public ISpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            return this;
        }

        public ISpanBuilder WithTag(IntTag tag, int value)
        {
            return this;
        }

        public ISpanBuilder WithTag(StringTag tag, string value)
        {
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            return this;
        }

        public IScope StartActive()
        {
            return tracer.ScopeManager.Activate(Start(), true);
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            return tracer.ScopeManager.Activate(Start(), finishSpanOnDispose);
        }

        public ISpan Start()
        {
            string traceId;
            if ( tracer.ActiveSpan == null)
            {
                traceId = Traktor.Util.generateNewId();
            }
            else
            {
                //explicit every new span is a childspan of active if active is != null
                this.AsChildOf(tracer.ScopeManager.Active.Span.Context);
                traceId = tracer.ActiveSpan.Context.TraceId;
            }
            string spanID = Traktor.Util.generateNewId();
            ISpanContext spanContext = new SpanContext(traceId, spanID);
            Span span = new Span(operationName, spanContext, references);
            return span;
        }

        public override string ToString()
        {
            return nameof(SpanBuilder);
        }
    }
}
