using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Base class for all exporters.
    /// </summary>
    public abstract class Exporter
    {
        /// <summary>
        /// Creates a new export.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="type">A type of export.</param>
        /// <param name="parameters">A parameters of export.</param>
        /// <param name="context">Expression context.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <returns>
        /// A new export.
        /// </returns>
        public abstract Export CreateExport(string name, string type, ParameterCollection parameters, EvaluationContext context, SpiceNetlistCaseSensitivitySettings caseSettings);
    }
}