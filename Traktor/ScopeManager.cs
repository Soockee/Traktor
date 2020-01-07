using System;
using System.Collections.Generic;
using System.Text;

using Traktor;
using OpenTracing;

namespace Traktor
{
    public sealed class ScopeManager : IScopeManager
    {
        public static readonly IScopeManager Instance = new ScopeManager();

        public List<IScope> inactiveScopes;

        private IScope active;

        public IScope Active { get { return active; } }

        public ScopeManager()
        {
            inactiveScopes = new List<IScope>();
        }

        public IScope Activate(ISpan span, bool finishSpanOnDispose)
        {
            IScope tmp = inactiveScopes.Find(scope => scope.Span == span);
            inactiveScopes.Add(active);
            ISpan newParent = active.Span;
            if (inactiveScopes.Remove(tmp))
            {
               // span.Context.TraceId = active.Span.Context.TraceId;
                active = tmp;
            }
            else {
                active = new Scope(span);
            }
            return active;
        }

        public override string ToString()
        {
            return nameof(ScopeManager);
        }

        public sealed class Scope : IScope
        {
            internal static readonly IScope Instance = new Scope(new Span("4head", new SpanContext(Traktor.Util.generateNewId(), Traktor.Util.generateNewId())));

            private ISpan span;
            public ISpan Span { get { return span; } }

            public Scope(ISpan span)
            {
                this.span = span;
            }

            public void Dispose()
            {
            }

            public override string ToString()
            {
                return nameof(Scope);
            }
        }
    }
}
