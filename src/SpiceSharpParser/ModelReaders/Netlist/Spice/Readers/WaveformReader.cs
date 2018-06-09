using SpiceSharp.Components;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    public class WaveformReader : IWaveformReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveformReader"/> class.
        /// </summary>
        /// <param name="registry">A waveform registry.</param>
        public WaveformReader(IWaveformRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the current waveform registry.
        /// </summary>
        public IWaveformRegistry Registry { get; }

        /// <summary>
        /// Gemerates wavefrom from bracket parameter.
        /// </summary>
        /// <param name="cp">A bracket parameter.</param>
        /// <param name="context">A processing context.</param>
        /// <returns>
        /// An new instance of waveform.
        /// </returns>
        public Waveform Generate(BracketParameter cp, IReadingContext context)
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
