using SpiceSharpParser.ModelsReaders.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the parser facade.
    /// </summary>
    public class ParserSettings
    {
        /// <summary>
        /// Gets or sets working directory path.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets the spice netlist parser settings.
        /// </summary>
        public SpiceNetlistParserSettings NetlistParser { get; } = new SpiceNetlistParserSettings();

        /// <summary>
        /// Gets the spice netlist model reader settings.
        /// </summary>
        public SpiceNetlistReaderSettings NetlistReader { get; } = new SpiceNetlistReaderSettings();
    }
}
