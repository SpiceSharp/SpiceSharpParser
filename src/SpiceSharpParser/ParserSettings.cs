using SpiceSharpParser.ModelReader.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the parser facade.
    /// </summary>
    public class ParserSettings
    {
        /// <summary>
        /// Gets the netlist parser settings.
        /// </summary>
        public SpiceNetlistParserSettings SpiceNetlistParserSettings { get; } = new SpiceNetlistParserSettings();

        /// <summary>
        /// Gets the spice model reader settings.
        /// </summary>
        public SpiceModelReaderSettings SpiceModelReaderSettings { get; } = new SpiceModelReaderSettings();
    }
}
