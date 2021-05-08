using System;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for pulse waveform.
    /// </summary>
    public class PulseGenerator : WaveformGenerator
    {
        /// <summary>
        /// Generates a new waveform.
        /// </summary>
        /// <param name="parameters">Parameters for waveform.</param>
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

            if (parameters.Count > 7 || parameters.Count == 1 && parameters[0] is VectorParameter vp && vp.Elements.Count > 7)
            {
                context.Result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        "Wrong number of arguments for PULSE waveform",
                        parameters.LineInfo));
            }

            var w = new Pulse();

            if (parameters.Count == 1 && parameters[0] is VectorParameter v)
            {
                if (v.Elements.Count >= 1)
                {
                    w.InitialValue = context.Evaluator.EvaluateDouble(v.Elements[0].Value);
                }

                if (v.Elements.Count >= 2)
                {
                    w.PulsedValue = context.Evaluator.EvaluateDouble(v.Elements[1].Value);
                }

                if (v.Elements.Count >= 3)
                {
                    w.Delay = context.Evaluator.EvaluateDouble(v.Elements[2].Value);
                }

                if (v.Elements.Count >= 4)
                {
                    w.RiseTime = context.Evaluator.EvaluateDouble(v.Elements[3].Value);
                }

                if (v.Elements.Count >= 5)
                {
                    w.FallTime = context.Evaluator.EvaluateDouble(v.Elements[4].Value);
                }

                if (v.Elements.Count >= 6)
                {
                    w.PulseWidth = context.Evaluator.EvaluateDouble(v.Elements[5].Value);
                }

                if (v.Elements.Count == 7)
                {
                    w.Period = context.Evaluator.EvaluateDouble(v.Elements[6].Value);
                }

                return w;
            }

            if (parameters.Count >= 1)
            {
                w.InitialValue = context.Evaluator.EvaluateDouble(parameters[0].Value);
            }

            if (parameters.Count >= 2)
            {
                w.PulsedValue = context.Evaluator.EvaluateDouble(parameters[1].Value);
            }

            if (parameters.Count >= 3)
            {
                w.Delay = context.Evaluator.EvaluateDouble(parameters[2].Value);
            }

            if (parameters.Count >= 4)
            {
                w.RiseTime = context.Evaluator.EvaluateDouble(parameters[3].Value);
            }

            if (parameters.Count >= 5)
            {
                w.FallTime = context.Evaluator.EvaluateDouble(parameters[4].Value);
            }

            if (parameters.Count >= 6)
            {
                w.PulseWidth = context.Evaluator.EvaluateDouble(parameters[5].Value);
            }

            if (parameters.Count == 7)
            {
                w.Period = context.Evaluator.EvaluateDouble(parameters[6].Value);
            }

            return w;
        }
    }
}