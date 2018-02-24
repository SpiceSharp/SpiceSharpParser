using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class WaveformProcessor
    {
        public WaveformProcessor(WaveformRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the current waveform registry
        /// </summary>
        public WaveformRegistry Registry { get; }

        public Waveform Generate(BracketParameter cp, ProcessingContext context)
        {
            string type = cp.Name.ToLower();
            if (!Registry.Supports(cp.Name))
            {
                throw new System.Exception("Unsupported waveform");
            }

            return Registry.Get(type).Generate(cp, context);
        }
    }
}
