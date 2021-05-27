using System;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

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
        public override IWaveformDescription Generate(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (parameters.Count > 6 || (parameters.Count == 1 && parameters[0] is VectorParameter vp && vp.Elements.Count > 6))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Wrong number of arguments for SINE waveform",
                    parameters.LineInfo);
            }

            var w = new Sine();

            if (parameters.Count == 1 && parameters[0] is VectorParameter v)
            {
                parameters = new ParameterCollection(v.Elements.Select(e => e).Cast<Parameter>().ToList());
            }

            if (parameters.Count >= 1)
            {
                w.Offset = context.Evaluator.EvaluateDouble(parameters[0].Value);
            }

            if (parameters.Count >= 2)
            {
                w.Amplitude = context.Evaluator.EvaluateDouble(parameters[1].Value);
            }

            if (parameters.Count >= 3)
            {
                w.Frequency = context.Evaluator.EvaluateDouble(parameters[2].Value);
            }

            if (parameters.Count >= 4)
            {
                w.Delay = context.Evaluator.EvaluateDouble(parameters[3].Value);
            }

            if (parameters.Count >= 5)
            {
                w.Theta = context.Evaluator.EvaluateDouble(parameters[4].Value);
            }

            if (parameters.Count >= 6)
            {
                w.Phase = context.Evaluator.EvaluateDouble(parameters[5].Value);
            }

            return w;
        }
    }
}