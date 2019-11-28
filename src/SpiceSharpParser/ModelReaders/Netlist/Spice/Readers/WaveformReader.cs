using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

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
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
        public Waveform Generate(string type, ParameterCollection parameters, ICircuitContext context)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!Mapper.TryGetValue(type, context.CaseSensitivity.IsFunctionNameCaseSensitive, out var reader))
            {
                throw new ReadingException("Unsupported waveform", parameters.LineNumber);
            }

            return reader.Generate(parameters, context);
        }

        public bool Supports(string type, ICircuitContext context)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (Mapper.TryGetValue(type, context.CaseSensitivity.IsFunctionNameCaseSensitive, out _))
            {
                return true;
            }

            return false;
        }
    }
}