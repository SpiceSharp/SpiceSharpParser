using System;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for sinusoidal waveform.
    /// </summary>
    public class SineGenerator : WaveformGenerator
    {
        /// <summary>
        /// Generates a new sinusoidal waveform.
        /// </summary>
        /// <param name="parameters">A parameter for waveform.</param>
        /// <param name="context">A context.</param>
        /// <returns>
        /// A new waveform.
        /// </returns>
        public override Waveform Generate(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (parameters.Count < 3 || parameters.Count > 6)
            {
                throw new WrongParametersCountException(
                    "Wrong parameters count for sine. There must be 3, 4, 5 or 6 parameters");
            }

            var sine = new Sine();

            sine.Offset.Value = context.EvaluateDouble(parameters.Get(0));
            sine.Amplitude.Value = context.EvaluateDouble(parameters.Get(1));
            sine.Frequency.Value = context.EvaluateDouble(parameters.Get(2));

            if (parameters.Count >= 4)
            {
                sine.Delay.Value = context.EvaluateDouble(parameters.Get(3));
            }

            if (parameters.Count >= 5)
            {
                sine.Theta.Value = context.EvaluateDouble(parameters.Get(4));
            }

            if (parameters.Count == 6)
            {
                sine.Phase.Value = context.EvaluateDouble(parameters.Get(5));
            }

            return sine;
        }
    }
}
