using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all waveform readers
    /// </summary>
    public interface IWaveformReader
    {
        /// <summary>
        /// Gemerates wavefrom from bracket parameter.
        /// </summary>
        /// <param name="cp">A bracket parameter.</param>
        /// <param name="context">A reading context.</param>
        /// <returns>
        /// An new instance of waveform.
        /// </returns>
        Waveform Generate(BracketParameter cp, IReadingContext context);
    }
}
