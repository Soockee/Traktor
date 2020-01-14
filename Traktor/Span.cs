using System;
using System.Collections.Generic;
using System.Text;

using OpenTracing;
using OpenTracing.Tag;

namespace Traktor
{
    public sealed class Span: ISpan
    {
        // Noop
        public ISpanContext Context { get { return spanContext; } }
        public static ISpanContext ContextStatic => SpanContext.Instance;
       // public static ISpan Instance = new Span("test-operation-name", ContextStatic, new Dictionary<string, string>());

        // Span Fields
        private string operationName;
        private DateTime startTimeStamp;
        private DateTime endTimeStamp;
        private ISpanContext spanContext;
        private Dictionary<string, string> references;

        // Tracer Reference to Report Span
        private Tracer tracer;

        public Span(string operationName, ISpanContext spanContext, Dictionary<string, string> references, Tracer tracer)
        {
            startTimeStamp = DateTime.UtcNow;
            this.operationName = operationName;
            this.spanContext = spanContext;
            this.references = references;
            this.endTimeStamp = DateTime.MinValue;
            this.tracer = tracer;
        }

        public void Finish()
        {
            if (endTimeStamp.Equals(DateTime.MinValue))
            {
                endTimeStamp = DateTime.UtcNow;
                tracer.reporter.Report(this);
            }
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
        }

        public ISpan SetTag(string key, string value)
        {
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            return this;
        }

        public ISpan SetTag(string key, int value)
        {
            return this;
        }

        public ISpan SetTag(string key, double value)
        {
            return this;
        }

        public ISpan SetTag(BooleanTag tag, bool value)
        {
            return this;
        }

        public ISpan SetTag(IntOrStringTag tag, string value)
        {
            return this;
        }

        public ISpan SetTag(IntTag tag, int value)
        {
            return this;
        }

        public ISpan SetTag(StringTag tag, string value)
        {
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            return this;
        }

        public ISpan Log(string @event)
        {
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            return this;
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return null;
        }

        public ISpan SetOperationName(string operationName)
        {
            return this;
        }

        public override string ToString()
        {
            string dateTimeFormat = "MM/dd/yyyy hh:mm:ss.ffff tt";
            string result = operationName + ";" + startTimeStamp.ToString(dateTimeFormat) +";"+ spanContext.ToString();
            if (!endTimeStamp.Equals(DateTime.MinValue))
            {
                result += ";" + endTimeStamp.ToString(dateTimeFormat);
            }
            return result;
        }
    }
}
