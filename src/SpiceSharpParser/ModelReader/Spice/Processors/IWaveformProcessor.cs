using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.Model.Spice.Objects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReader.Spice.Processors
{
    /// <summary>
    /// Interface for all waveform processors
    /// </summary>
    public interface IWaveformProcessor
    {
        /// <summary>
        /// Gemerates wavefrom from bracket parameter
        /// </summary>
        /// <param name="cp">A bracket parameter</param>
        /// <param name="context">A processing context</param>
        /// <returns>
        /// An new instance of waveform
        /// </returns>
        Waveform Generate(BracketParameter cp, IProcessingContext context);
    }
}
