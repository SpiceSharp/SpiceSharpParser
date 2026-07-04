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

            parameters = FlattenVector(parameters);
            var argumentCount = parameters.Count;

            if (argumentCount > 8 || (argumentCount == 8 && !context.ReaderSettings.Compatibility.IsLTspice))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Wrong number of arguments for PULSE waveform",
                    parameters.LineInfo);
                return null;
            }

            if (argumentCount == 8)
            {
                return GenerateFinitePulse(parameters, context);
            }

            var w = new Pulse();

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

        private static IWaveformDescription GenerateFinitePulse(ParameterCollection parameters, IReadingContext context)
        {
            var waveform = new FinitePulse
            {
                InitialValue = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[0].Value),
                PulsedValue = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[1].Value),
                Delay = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[2].Value),
                RiseTime = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[3].Value),
                FallTime = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[4].Value),
                PulseWidth = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[5].Value),
                Period = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[6].Value),
                CycleCount = context.EvaluationContext.Evaluator.EvaluateDouble(parameters[7].Value),
            };

            if (waveform.Period <= 0.0 || double.IsNaN(waveform.Period) || double.IsInfinity(waveform.Period))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "LTspice finite-cycle PULSE period must be positive.",
                    parameters[6].LineInfo);
                return null;
            }

            if (waveform.CycleCount <= 0.0 || double.IsNaN(waveform.CycleCount) || double.IsInfinity(waveform.CycleCount))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "LTspice finite-cycle PULSE cycle-count must be positive.",
                    parameters[7].LineInfo);
                return null;
            }

            return waveform;
        }

        private static ParameterCollection FlattenVector(ParameterCollection parameters)
        {
            if (parameters.Count == 1 && parameters[0] is VectorParameter vector)
            {
                return new ParameterCollection(vector.Elements.Select(element => element).Cast<Parameter>().ToList());
            }

            return parameters;
        }
    }
}
