using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// A result of the SPICE netlist parser.
    /// </summary>
    public class SpiceNetlistParseResult
    {
        /// <summary>
        /// Gets or sets the netlist model before preprocessing.
        /// </summary>
        public SpiceNetlist InputModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after processing.
        /// </summary>
        public SpiceNetlist FinalModel { get; set; }

        /// <summary>
        /// Gets or sets validation result.
        /// </summary>
        public ValidationEntryCollection ValidationResult { get; set; }
    }
}