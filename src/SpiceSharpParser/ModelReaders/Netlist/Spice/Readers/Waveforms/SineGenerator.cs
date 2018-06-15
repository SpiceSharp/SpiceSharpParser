using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for sinusoidal waveform
    /// </summary>
    public class SineGenerator : WaveformGenerator
    {
        public override string SpiceCommandName => "sine";

        /// <summary>
        /// Generats a new sinusoidal waveform
        /// </summary>
        /// <param name="bracketParameter">A parameter for waveform</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new waveform
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
