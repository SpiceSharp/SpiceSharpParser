using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms
{
    /// <summary>
    /// Writer for sinusoidal waveform.
    /// </summary>
    public class SineWriter : BaseWriter, IWaveformWriter
    {
        /// <summary>
        /// Generates a new sinusoidal waveform.
        /// </summary>
        /// <param name="parameters">A parameter for waveform.</param>
        /// <param name="context">A context.</param>
        /// <param name="waveformId">Waveform id.</param>
        /// <returns>
        /// A new waveform.
        /// </returns>
        public List<CSharpStatement> Generate(ParameterCollection parameters, IWriterContext context, out string waveformId)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = new List<CSharpStatement>();

            if (parameters.Count > 6 || (parameters.Count == 1 && parameters[0] is VectorParameter vp && vp.Elements.Count > 6))
            {
                waveformId = null;

                result.Add(new CSharpComment("Skipped, wrong parameters count for SINE waveform"));
                return result;
            }

            waveformId = context.GetNewIdentifier("sine");
            result.Add(new CSharpNewStatement(waveformId, "new Sine()") { IncludeInCollection = false });

            if (parameters.Count == 1 && parameters[0] is VectorParameter v)
            {
                parameters = new ParameterCollection(v.Elements.Select(e => e).Cast<Parameter>().ToList());
            }

            if (parameters.Count >= 1)
            {
                result.Add(SetProperty(waveformId, "Offset", parameters[0].Value, context));
            }

            if (parameters.Count >= 2)
            {
                result.Add(SetProperty(waveformId, "Amplitude", parameters[1].Value, context));
            }

            if (parameters.Count >= 3)
            {
                result.Add(SetProperty(waveformId, "Frequency", parameters[2].Value, context));
            }

            if (parameters.Count >= 4)
            {
                result.Add(SetProperty(waveformId, "Delay", parameters[3].Value, context));
            }

            if (parameters.Count >= 5)
            {
                result.Add(SetProperty(waveformId, "Theta", parameters[4].Value, context));
            }

            if (parameters.Count >= 6)
            {
                result.Add(SetProperty(waveformId, "Phase", parameters[5].Value, context));
            }

            return result;
        }
    }
}