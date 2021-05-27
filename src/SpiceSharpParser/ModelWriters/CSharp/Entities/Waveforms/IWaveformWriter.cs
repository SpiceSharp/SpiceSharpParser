using System.Collections.Generic;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms
{
    /// <summary>
    /// Generates a waveform.
    /// </summary>
    public interface IWaveformWriter
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
        public List<CSharpStatement> Generate(ParameterCollection parameters, IWriterContext context, out string waveformId);
    }
}