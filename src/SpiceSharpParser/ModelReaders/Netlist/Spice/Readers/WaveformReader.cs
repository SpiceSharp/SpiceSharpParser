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
        /// <param name="mapper">A waveform mapper.</param>
        public WaveformReader(IMapper<WaveformGenerator> mapper)
        {
            Mapper = mapper;
        }

        /// <summary>
        /// Gets the waveform mapper.
        /// </summary>
        public IMapper<WaveformGenerator> Mapper { get; }

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
            if (!Mapper.Contains(type))
            {
                throw new System.Exception("Unsupported waveform");
            }

            return Mapper.Get(type).Generate(cp, context);
        }
    }
}
