using System;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

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

            if (parameters.Count != 7)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong number of arguments for pulse", parameters.LineInfo));
                return null;
            }

            var w = new Pulse();
            w.InitialValue = context.Evaluator.EvaluateDouble(parameters.Get(0));
            w.PulsedValue = context.Evaluator.EvaluateDouble(parameters.Get(1));
            w.Delay = context.Evaluator.EvaluateDouble(parameters.Get(2));
            w.RiseTime = context.Evaluator.EvaluateDouble(parameters.Get(3));
            w.FallTime = context.Evaluator.EvaluateDouble(parameters.Get(4));
            w.PulseWidth = context.Evaluator.EvaluateDouble(parameters.Get(5));
            w.Period = context.Evaluator.EvaluateDouble(parameters.Get(6));

            return w;
        }
    }
}