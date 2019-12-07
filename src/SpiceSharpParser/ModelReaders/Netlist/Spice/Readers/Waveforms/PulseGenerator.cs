using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

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
        public override Waveform Generate(ParameterCollection parameters, ICircuitContext context)
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
                throw new WrongParametersCountException("Wrong number of arguments for pulse", parameters.LineInfo);
            }

            var w = new Pulse();
            w.InitialValue.Value = context.Evaluator.EvaluateDouble(parameters.Get(0));
            w.PulsedValue.Value = context.Evaluator.EvaluateDouble(parameters.Get(1));
            w.Delay.Value = context.Evaluator.EvaluateDouble(parameters.Get(2));
            w.RiseTime.Value = context.Evaluator.EvaluateDouble(parameters.Get(3));
            w.FallTime.Value = context.Evaluator.EvaluateDouble(parameters.Get(4));
            w.PulseWidth.Value = context.Evaluator.EvaluateDouble(parameters.Get(5));
            w.Period.Value = context.Evaluator.EvaluateDouble(parameters.Get(6));

            return w;
        }
    }
}