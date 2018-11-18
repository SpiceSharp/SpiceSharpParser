using System;
using System.Collections.Generic;

using SpiceSharp.Components;
using SpiceSharp.Components.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

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
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            bool vectorMode = false;
            if (parameters.Count > 1 && parameters[1] is VectorParameter vp && vp.Elements.Count == 2)
            {
                vectorMode = true;
            }

            if (!vectorMode)
            {
                if (parameters.Count % 2 != 0)
                {
                    throw new ArgumentException("PWL waveform expects even count of parameters");
                }

                double[] times = new double[parameters.Count / 2];
                double[] voltages = new double[parameters.Count / 2];

                for (var i = 0; i < parameters.Count / 2; i++)
                {
                    times[i] = context.EvaluateDouble(parameters.GetString(2 * i));
                    voltages[i] = context.EvaluateDouble(parameters.GetString((2 * i) + 1));
                }

                var pwl = new Pwl(times, voltages);
                return pwl;
            }
            else
            {
                List<double> values = new List<double>();

                for (var i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i] is VectorParameter vp2 && vp2.Elements.Count == 2)
                    {
                        values.Add(context.EvaluateDouble(vp2.Elements[0].Image));
                        values.Add(context.EvaluateDouble(vp2.Elements[1].Image));
                    }
                    else
                    {
                        values.Add(context.EvaluateDouble(parameters[i].Image));
                    }
                }

                int pwlPoints = values.Count / 2;
                double[] times = new double[pwlPoints];
                double[] voltages = new double[pwlPoints];

                for (var i = 0; i < pwlPoints; i++)
                {
                    times[i] = values[2 * i];
                    voltages[i] = values[(2 * i) + 1];
                }

                var pwl = new Pwl(times, voltages);
                return pwl;
            }
        }
    }
}
