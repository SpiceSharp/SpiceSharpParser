using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generates a waveform
    /// </summary>
    public abstract class WaveformGenerator : ISpiceObjectReader
    {
        /// <summary>
        /// Gets the type name of generated waveform
        /// </summary>
        public abstract string SpiceName { get; }

        /// <summary>
        /// Generats a new waveform
        /// </summary>
        /// <param name="bracketParameter">A parameter for waveform</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new waveform
        /// </returns>
        public abstract Waveform Generate(BracketParameter bracketParameter, IReadingContext context);
    }
}
