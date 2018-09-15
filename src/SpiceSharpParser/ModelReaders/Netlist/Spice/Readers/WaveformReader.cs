using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    public class WaveformReader : IWaveformReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveformReader"/> class.
        /// </summary>
        /// <param name="registry">A waveform registry.</param>
        public WaveformReader(IRegistry<WaveformGenerator> registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the current waveform registry.
        /// </summary>
        public IRegistry<WaveformGenerator> Registry { get; }

        /// <summary>
        /// Gemerates wavefrom from bracket parameter.
        /// </summary>
        /// <param name="cp">A bracket parameter.</param>
        /// <param name="context">A reading context.</param>
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
