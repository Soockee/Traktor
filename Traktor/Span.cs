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
        public string operationName;
        public DateTime startTimeStamp;
        public DateTime endTimeStamp;
        public ISpanContext spanContext;
        public Dictionary<string, string> references;

        public Span(string operationName, ISpanContext spanContext, Dictionary<string, string> references)
        {
            // startTimeStamp = 
            this.operationName = operationName;
            this.spanContext = spanContext;
            this.references = references;
        }

        public void Finish()
        {
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
            string result = operationName + ";" + startTimeStamp.ToString() +";"+ spanContext.ToString();
            if (endTimeStamp != null) result += ";" + endTimeStamp.ToString();
            return result;
        }
    }
}
