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
    /// Generator for exponential waveform.
    /// </summary>
    public class ExpGenerator : WaveformGenerator
    {
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

            if (parameters.Count != 6)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "EXP waveform expects exactly six arguments: EXP(v1 v2 td1 tau1 td2 tau2).",
                    parameters.LineInfo);
                return null;
            }

            var waveform = new Exponential
            {
                InitialValue = context.Evaluator.EvaluateDouble(parameters[0].Value),
                PulsedValue = context.Evaluator.EvaluateDouble(parameters[1].Value),
                RiseDelay = context.Evaluator.EvaluateDouble(parameters[2].Value),
                RiseTimeConstant = context.Evaluator.EvaluateDouble(parameters[3].Value),
                FallDelay = context.Evaluator.EvaluateDouble(parameters[4].Value),
                FallTimeConstant = context.Evaluator.EvaluateDouble(parameters[5].Value),
            };

            if (waveform.RiseTimeConstant <= 0.0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "EXP waveform rise time constant tau1 must be positive.",
                    parameters[3].LineInfo);
                return null;
            }

            if (waveform.FallTimeConstant <= 0.0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "EXP waveform fall time constant tau2 must be positive.",
                    parameters[5].LineInfo);
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
