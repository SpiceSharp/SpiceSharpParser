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

            var argumentCount = parameters.Count == 1 && parameters[0] is VectorParameter vp
                ? vp.Elements.Count
                : parameters.Count;

            if (argumentCount > 7)
            {
                if (context.ReaderSettings.Compatibility.IsLTspice)
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        "Unsupported LTspice PULSE cycle-count arguments: finite-cycle PULSE sources are not mapped yet.",
                        parameters.LineInfo);
                    return null;
                }

                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Wrong number of arguments for PULSE waveform",
                    parameters.LineInfo);
            }

            var w = new Pulse();

            if (parameters.Count == 1 && parameters[0] is VectorParameter v)
            {
                parameters = new ParameterCollection(v.Elements.Select(e => e).Cast<Parameter>().ToList());
            }

            if (parameters.Count >= 1)
            {
                w.InitialValue = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[0].Value);
            }

            if (parameters.Count >= 2)
            {
                w.PulsedValue = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[1].Value);
            }

            if (parameters.Count >= 3)
            {
                w.Delay = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[2].Value);
            }

            if (parameters.Count >= 4)
            {
                w.RiseTime = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[3].Value);
            }

            if (parameters.Count >= 5)
            {
                w.FallTime = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[4].Value);
            }

            if (parameters.Count >= 6)
            {
                w.PulseWidth = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[5].Value);
            }

            if (parameters.Count == 7)
            {
                w.Period = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[6].Value);
            }

            return w;
        }
    }
}
