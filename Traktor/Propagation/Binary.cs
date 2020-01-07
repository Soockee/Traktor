using System.IO; 
using OpenTracing.Propagation;

namespace Traktor.Propagation
{
    public class Binary: IBinary
    {
        public void Set(MemoryStream stream) 
        { 
        
        }
        public MemoryStream Get() 
        {
            return null;
        }
    }
}
