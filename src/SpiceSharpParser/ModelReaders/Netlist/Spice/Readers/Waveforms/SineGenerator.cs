using System;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
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
        public override IWaveformDescription Generate(ParameterCollection parameters, ICircuitContext context)
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
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameters count for sine. There must be 3, 4, 5 or 6 parameters", parameters.LineInfo));
            }

            var sine = new Sine();

            sine.Offset = context.Evaluator.EvaluateDouble(parameters.Get(0));
            sine.Amplitude = context.Evaluator.EvaluateDouble(parameters.Get(1));
            sine.Frequency = context.Evaluator.EvaluateDouble(parameters.Get(2));

            if (parameters.Count >= 4)
            {
                sine.Delay = context.Evaluator.EvaluateDouble(parameters.Get(3));
            }

            if (parameters.Count >= 5)
            {
                sine.Theta = context.Evaluator.EvaluateDouble(parameters.Get(4));
            }

            if (parameters.Count == 6)
            {
                sine.Phase = context.Evaluator.EvaluateDouble(parameters.Get(5));
            }

            return sine;
        }
    }
}