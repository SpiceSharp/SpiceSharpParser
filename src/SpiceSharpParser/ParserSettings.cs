using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the parser.
    /// </summary>
    public class ParserSettings
    {
        /// <summary>
        /// Gets or sets a path of working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets the SPICE netlist parser settings.
        /// </summary>
        public SpiceNetlistParserSettings NetlistParser { get; } = new SpiceNetlistParserSettings();

        /// <summary>
        /// Gets the SPICE netlist model reader settings.
        /// </summary>
        public SpiceNetlistReaderSettings NetlistReader { get; } = new SpiceNetlistReaderSettings();
    }
}
