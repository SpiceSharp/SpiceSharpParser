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
    /// Generator for AM waveform.
    /// </summary>
    public class AMGenerator : WaveformGenerator
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

            if (parameters.Count > 7 || (parameters.Count == 1 && parameters[0] is VectorParameter vp && vp.Elements.Count > 7))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Wrong number of arguments for AM waveform",
                    parameters.LineInfo);
            }

            var w = new AM();

            if (parameters.Count == 1 && parameters[0] is VectorParameter v)
            {
                parameters = new ParameterCollection(v.Elements.Select(e => e).Cast<Parameter>().ToList());
            }

            if (parameters.Count >= 1)
            {
                w.Amplitude = context.Evaluator.EvaluateDouble(parameters[0].Value);
            }

            if (parameters.Count >= 2)
            {
                w.Offset = context.Evaluator.EvaluateDouble(parameters[1].Value);
            }

            if (parameters.Count >= 3)
            {
                w.ModulationFrequency = context.Evaluator.EvaluateDouble(parameters[2].Value);
            }

            if (parameters.Count >= 4)
            {
                w.CarrierFrequency = context.Evaluator.EvaluateDouble(parameters[3].Value);
            }

            if (parameters.Count >= 5)
            {
                w.SignalDelay = context.Evaluator.EvaluateDouble(parameters[4].Value);
            }

            if (parameters.Count >= 6)
            {
                w.CarrierPhase = context.Evaluator.EvaluateDouble(parameters[5].Value);
            }

            if (parameters.Count == 7)
            {
                w.SignalPhase = context.Evaluator.EvaluateDouble(parameters[6].Value);
            }

            return w;
        }
    }
}