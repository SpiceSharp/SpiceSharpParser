using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    public class WaveformProcessor : IWaveformProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveformProcessor"/> class.
        /// </summary>
        /// <param name="registry">A waveform registry</param>
        public WaveformProcessor(IWaveformRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the current waveform registry
        /// </summary>
        public IWaveformRegistry Registry { get; }

        /// <summary>
        /// Gemerates wavefrom from bracket parameter
        /// </summary>
        /// <param name="cp">A bracket parameter</param>
        /// <param name="context">A processing context</param>
        /// <returns>
        /// An new instance of waveform
        /// </returns>
        public Waveform Generate(BracketParameter cp, IProcessingContext context)
        {
            string type = cp.Name.ToLower();
            if (!Registry.Supports(type))
            {
                throw new System.Exception("Unsupported waveform");
            }

            return Registry.Get(type).Generate(cp, context);
        }
    }
}
