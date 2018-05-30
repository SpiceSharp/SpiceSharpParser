using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
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
