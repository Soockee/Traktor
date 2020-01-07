using OpenTracing;
namespace Traktor
{
    /// <summary>
    /// Provides access to the singleton <see cref="NoopTracer"/> instance.
    /// </summary>
    public static class TracerFactory
    {
        /// <summary>
        /// Returns the singleton <see cref="NoopTracer"/> instance.
        /// </summary>
        public static ITracer Create()
        {
            return Tracer.Instance;
        }
    }
}