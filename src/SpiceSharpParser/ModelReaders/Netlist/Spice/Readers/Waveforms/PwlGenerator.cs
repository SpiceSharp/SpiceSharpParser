using SpiceSharp.Components;
using SpiceSharp.Components.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for PWL waveform.
    /// </summary>
    public class PwlGenerator : WaveformGenerator
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
            double[] times = new double[parameters.Count / 2];
            double[] voltages = new double[parameters.Count / 2];

            for (var i = 0; i < parameters.Count / 2; i++)
            {
                times[i] = context.EvaluateDouble(parameters.GetString(2*i));
                voltages[i] = context.EvaluateDouble(parameters.GetString(2*i+1));
            }

            var w = new Pwl(times, voltages);

            return w;
        }
    }
}
