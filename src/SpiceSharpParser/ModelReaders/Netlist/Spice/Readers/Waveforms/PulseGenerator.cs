using SpiceSharp.Components;
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
        public override Waveform Generate(ParameterCollection parameters, IReadingContext context)
        {
            var w = new Pulse();

            if (parameters.Count != 7)
            {
                throw new System.Exception("Wrong number of arguments for pulse");
            }

            w.InitialValue.Value = context.EvaluateDouble(parameters.GetString(0));
            w.PulsedValue.Value = context.EvaluateDouble(parameters.GetString(1));
            w.Delay.Value = context.EvaluateDouble(parameters.GetString(2));
            w.RiseTime.Value = context.EvaluateDouble(parameters.GetString(3));
            w.FallTime.Value = context.EvaluateDouble(parameters.GetString(4));
            w.PulseWidth.Value = context.EvaluateDouble(parameters.GetString(5));
            w.Period.Value = context.EvaluateDouble(parameters.GetString(6));

            return w;
        }
    }
}
