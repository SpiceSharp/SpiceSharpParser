using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms
{
    /// <summary>
    /// Single-frequency FM waveform.
    /// </summary>
    public class SFFMWriter : BaseWriter, IWaveformWriter
    {
        /// <summary>
        /// Generates a new waveform.
        /// </summary>
        /// <param name="parameters">Parameters for waveform.</param>
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

            if (parameters.Count > 7 || (parameters.Count == 1 && parameters[0] is VectorParameter vp && vp.Elements.Count > 7))
            {
                waveformId = null;
                result.Add(new CSharpComment("Skipped, wrong parameters count for SFFM waveform"));
                return result;
            }

            waveformId = context.GetNewIdentifier("sffm");
            result.Add(new CSharpNewStatement(waveformId, "new SFFM()") { IncludeInCollection = false });

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
                result.Add(SetProperty(waveformId, "CarrierFrequency", parameters[2].Value, context));
            }

            if (parameters.Count >= 4)
            {
                result.Add(SetProperty(waveformId, "ModulationIndex", parameters[3].Value, context));
            }

            if (parameters.Count >= 5)
            {
                result.Add(SetProperty(waveformId, "SignalFrequency", parameters[4].Value, context));
            }

            if (parameters.Count >= 6)
            {
                result.Add(SetProperty(waveformId, "CarrierPhase", parameters[5].Value, context));
            }

            if (parameters.Count == 7)
            {
                result.Add(SetProperty(waveformId, "SignalPhase", parameters[5].Value, context));
            }

            return result;
        }
    }
}
