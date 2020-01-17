using System.IO;
using System;
using OpenTracing.Propagation;

namespace Traktor.Propagation
{
    public class Binary: IBinary, IDisposable
    {
        MemoryStream context;
        // NICHT VERGESSEN ! STREAM Schließen
          


        public void Set(MemoryStream stream) 
        {
            context = stream;
        }
        public MemoryStream Get() 
        {
            return context;
        }

        public void Dispose() 
        { 
            
        }
    }
}
