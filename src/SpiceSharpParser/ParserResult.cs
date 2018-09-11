using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// A parser result.
    /// </summary>
    public class ParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserResult"/> class.
        /// </summary>
        public ParserResult()
        {
        }

        /// <summary>
        /// Gets or sets the result of reading <see cref="SpiceNetlistReaderResult"/> model.
        /// </summary>
        public SpiceNetlistReaderResult Result { get; set; }

        /// <summary>
        /// Gets or sets the netlist model before preprocessing.
        /// </summary>
        public SpiceNetlist InitialNetlistModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after preprocessing.
        /// </summary>
        public SpiceNetlist PreprocessedNetlistModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after postprocessing.
        /// </summary>
        public SpiceNetlist PostprocessedNetlistModel { get; set; }
    }
}
