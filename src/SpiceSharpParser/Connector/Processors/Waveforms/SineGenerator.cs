using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.Connector.Processors.Waveforms
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

            if (bracketParameter.Parameters.Count < 3 || bracketParameter.Parameters.Count > 5)
            {
                throw new WrongParametersCountException("Wrong parameters count for sine. There must be 3,4 or 5 parameters");
            }
            else
            {
                sine.Offset.Value = context.ParseDouble(bracketParameter.Parameters.GetString(0));
                sine.Amplitude.Value = context.ParseDouble(bracketParameter.Parameters.GetString(1));
                sine.Frequency.Value = context.ParseDouble(bracketParameter.Parameters.GetString(2));
            }

            if (bracketParameter.Parameters.Count >= 4)
            {
                sine.Delay.Value = context.ParseDouble(bracketParameter.Parameters.GetString(3));
            }

            if (bracketParameter.Parameters.Count == 5)
            {
                sine.Theta.Value = context.ParseDouble(bracketParameter.Parameters.GetString(4));
            }

            return sine;
        }
    }
}
