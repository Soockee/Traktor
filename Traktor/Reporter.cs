using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

using Traktor;
using OpenTracing;
namespace Traktor
{
    class Reporter
    {
        ITracer _tracer;
        string adress;
        int port;
        public Reporter(string adress, int port,  ITracer _tracer) 
        {
            this._tracer = _tracer;
            this.adress = adress;
            this.port = port;
        }

        public void Report(ISpan span) 
        { 
            
        }
    }
}
