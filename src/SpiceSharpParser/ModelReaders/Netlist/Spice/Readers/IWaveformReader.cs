using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all waveform readers.
    /// </summary>
    public interface IWaveformReader
    {
        /// <summary>
        /// Generates waveform from bracket parameter.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="context">A reading context.</param>
        /// <returns>
        /// An new instance of waveform.
        /// </returns>
        IWaveformDescription Generate(string type, ParameterCollection parameters, IReadingContext context);

        bool Supports(string type, IReadingContext context);
    }
}