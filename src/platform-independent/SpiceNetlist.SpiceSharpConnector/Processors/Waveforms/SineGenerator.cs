using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;
using SpiceNetlist.SpiceSharpConnector.Context;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Waveforms
{
    /// <summary>
    /// Generator for sinusoidal waveform
    /// </summary>
    public class SineGenerator : WaveformGenerator
    {
        public override string TypeName => "sine";

        /// <summary>
        /// Generats a new sinusoidal waveform
        /// </summary>
        /// <param name="bracketParameter">A parameter for waveform</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new waveform
        /// </returns>
        public override Waveform Generate(BracketParameter bracketParameter, IProcessingContext context)
        {
            var sine = new Sine();

            sine.Offset.Value = context.ParseDouble(bracketParameter.Parameters.GetString(0));
            sine.Amplitude.Value = context.ParseDouble(bracketParameter.Parameters.GetString(1));
            sine.Frequency.Value = context.ParseDouble(bracketParameter.Parameters.GetString(2));
            sine.Delay.Value = context.ParseDouble(bracketParameter.Parameters.GetString(3));
            sine.Theta.Value = context.ParseDouble(bracketParameter.Parameters.GetString(4));

            return sine;
        }
    }
}
