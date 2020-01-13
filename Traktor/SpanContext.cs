using System.Collections.Generic;
using System.Linq;

using OpenTracing;

namespace Traktor
{
    
    public sealed class SpanContext: ISpanContext
    {
        internal static readonly ISpanContext Instance = new SpanContext("0001", "9998");

        //SpanContext Fields
        public string traceId;
        public string spanId;
        private string referencetyp;
       

        public SpanContext(string traceId, string spanId) 
        {
            this.traceId = traceId;
            this.spanId = spanId;
            this.referencetyp = References.ChildOf;
        }
        public SpanContext(string traceId, string spanId, string referencetyp)
        {
            this.traceId = traceId;
            this.spanId = spanId;
            this.referencetyp = referencetyp;
        }

        public string TraceId { get { return traceId; } }

        public string SpanId { get {return spanId; } }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public override string ToString()
        {
            return traceId+";"+spanId+";"+referencetyp;
        }

    }
}
