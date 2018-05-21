using SpiceSharpParser.Model;

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
        /// Gets or sets the Spice# model.
        /// </summary>
        public Connector.SpiceSharpModel SpiceSharpModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model before preprocessing.
        /// </summary>
        public Model.Netlist InitialNetlistModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after preprocessing.
        /// </summary>
        public Model.Netlist PreprocessedNetlistModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after postprocessing.
        /// </summary>
        public Model.Netlist PostprocessedNetlistModel { get; set; }
    }
}
