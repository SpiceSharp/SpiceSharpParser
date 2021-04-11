using System;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Validation;
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
        public IWaveformDescription Generate(string type, ParameterCollection parameters, ICircuitContext context)
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
                context.Result.Validation.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Unsupported waveform '{type}'",
                        parameters.LineInfo));

                return null;
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