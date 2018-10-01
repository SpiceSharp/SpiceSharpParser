using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// A result of the SPICE netlist parser.
    /// </summary>
    public class SpiceParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParserResult"/> class.
        /// </summary>
        public SpiceParserResult()
        {
        }

        /// <summary>
        /// Gets or sets the result of reading <see cref="SpiceNetlistReaderResult"/> model.
        /// </summary>
        public SpiceNetlistReaderResult SpiceSharpModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model before preprocessing.
        /// </summary>
        public SpiceNetlist OriginalInputModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after preprocessing.
        /// </summary>
        public SpiceNetlist PreprocessedInputModel { get; set; }
    }
}
