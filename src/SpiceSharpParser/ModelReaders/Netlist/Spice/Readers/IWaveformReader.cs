using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all waveform readers
    /// </summary>
    public interface IWaveformReader
    {
        /// <summary>
        /// Gemerates wavefrom from bracket parameter
        /// </summary>
        /// <param name="cp">A bracket parameter</param>
        /// <param name="context">A processing context</param>
        /// <returns>
        /// An new instance of waveform
        /// </returns>
        Waveform Generate(BracketParameter cp, IReadingContext context);
    }
}
