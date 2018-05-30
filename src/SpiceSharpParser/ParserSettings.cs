using SpiceSharpParser.ModelReader.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the parser facade.
    /// </summary>
    public class ParserSettings
    {
        /// <summary>
        /// Gets the spice netlist parser settings.
        /// </summary>
        public SpiceNetlistParserSettings SpiceNetlistParserSettings { get; } = new SpiceNetlistParserSettings();

        /// <summary>
        /// Gets the spice netlist model reader settings.
        /// </summary>
        public SpiceNetlistReaderSettings SpiceNetlistModelReaderSettings { get; } = new SpiceNetlistReaderSettings();
    }
}
