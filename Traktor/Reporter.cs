using OpenTracing;
using System;
using System.Net.Sockets;
using System.Text;
namespace Traktor
{
    public class Reporter
    {
        private ITracer _tracer;
        private string agentaddress;
        private int reporterport;
        private UdpClient agent;
        public Reporter(string agentaddress, int agentport, int reporterport, ITracer _tracer)
        {
            this._tracer = _tracer;
            this.agentaddress = agentaddress;
            this.reporterport = reporterport;
            this.agent = Connect(agentaddress, agentport);
        }

        public void Report(ISpan span)
        {
            Byte[] message = BuildMessage(span);
            try
            {
                agent.Send(message, message.Length);
            }
            catch (SocketException se)
            {
                throw se;
            }
        }

        private UdpClient Connect(string agentaddress, int agentport)
        {
            UdpClient udpclient = new UdpClient(reporterport);
            try
            {
                udpclient.Connect(agentaddress, agentport);
            }
            catch (SocketException se)
            {
                throw se;
            }
            return udpclient;
        }

        private Byte[] BuildMessage(ISpan span)
        {
            return Encoding.UTF8.GetBytes(span.ToString());
        }
    }
}
