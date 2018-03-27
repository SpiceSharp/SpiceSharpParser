using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Common;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.Connector.Processors.Waveforms
{
    /// <summary>
    /// Generates a waveform
    /// </summary>
    public abstract class WaveformGenerator : IGenerator
    {
        /// <summary>
        /// Gets the type name of generated waveform
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// Generats a new waveform
        /// </summary>
        /// <param name="bracketParameter">A parameter for waveform</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new waveform
        /// </returns>
        public abstract Waveform Generate(BracketParameter bracketParameter, IProcessingContext context);
    }
}
