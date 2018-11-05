using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

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
        /// Generates a waveform from bracket parameter.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="context">A reading context.</param>
        /// <returns>
        /// An new instance of waveform.
        /// </returns>
        public Waveform Generate(string type, ParameterCollection parameters, IReadingContext context)
        {
            if (!Mapper.TryGetValue(type, context.CaseSensitivity.IsFunctionNameCaseSensitive, out var reader))
            {
                throw new System.Exception("Unsupported waveform");
            }

            return reader.Generate(parameters, context);
        }

        public bool Supports(string type, IReadingContext context)
        {
            if (Mapper.TryGetValue(type, context.CaseSensitivity.IsFunctionNameCaseSensitive, out var reader))
            {
                return true;
            }
            return false;
        }
    }
}
