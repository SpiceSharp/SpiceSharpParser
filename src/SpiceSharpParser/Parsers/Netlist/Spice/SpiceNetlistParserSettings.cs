using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the SPICE netlist parser.
    /// </summary>
    public class SpiceNetlistParserSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether '.END' is required at the end of the netlist.
        /// </summary>
        public bool IsEndRequired { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether new line characters are required at the end of the netlist.
        /// </summary>
        public bool IsNewlineRequired { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether netlist has a title at the first line.
        /// </summary>
        public bool HasTitle { get; set; } = true;
    }
}
