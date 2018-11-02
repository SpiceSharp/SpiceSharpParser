using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for sinusoidal waveform
    /// </summary>
    public class SineGenerator : WaveformGenerator
    {
        /// <summary>
        /// Generates a new sinusoidal waveform.
        /// </summary>
        /// <param name="bracketParameter">A parameter for waveform.</param>
        /// <param name="context">A context.</param>
        /// <returns>
        /// A new waveform.
        /// </returns>
        public override Waveform Generate(BracketParameter bracketParameter, IReadingContext context)
        {
            var sine = new Sine();

            if (bracketParameter.Parameters.Count < 3 || bracketParameter.Parameters.Count > 5)
            {
                throw new WrongParametersCountException("Wrong parameters count for sine. There must be 3,4 or 5 parameters");
            }
            else
            {
                sine.Offset.Value = context.EvaluateDouble(bracketParameter.Parameters.GetString(0));
                sine.Amplitude.Value = context.EvaluateDouble(bracketParameter.Parameters.GetString(1));
                sine.Frequency.Value = context.EvaluateDouble(bracketParameter.Parameters.GetString(2));
            }

            if (bracketParameter.Parameters.Count >= 4)
            {
                sine.Delay.Value = context.EvaluateDouble(bracketParameter.Parameters.GetString(3));
            }

            if (bracketParameter.Parameters.Count == 5)
            {
                sine.Theta.Value = context.EvaluateDouble(bracketParameter.Parameters.GetString(4));
            }

            return sine;
        }
    }
}
