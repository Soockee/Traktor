using OpenTracing.Propagation;
using System;
using System.IO;
using System.Text;

namespace Traktor.Propagation
{
    [Serializable]
    public class BinaryCarrier : IBinary, IDisposable
    {
        MemoryStream context;

        public BinaryCarrier()
        {
        }
        public BinaryCarrier(MemoryStream context)
        {
            this.context = context;
        }

        public void Set(MemoryStream stream)
        {
            context = stream;
        }
        public MemoryStream Get()
        {
            return context;
        }
        public string toString()
        {
            return Encoding.ASCII.GetString(context.ToArray());
        }
        public void Dispose()
        {
            context.Close();
        }
    }
}
