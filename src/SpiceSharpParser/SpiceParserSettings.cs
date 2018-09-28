using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the parser.
    /// </summary>
    public class SpiceParserSettings
    {
        public SpiceParserSettings()
        {
            CaseSensitivity = new CaseSensitivitySettings();
            Parsing = new SpiceNetlistParserSettings(CaseSensitivity);
            Reading = new SpiceNetlistReaderSettings(CaseSensitivity);
        }

        /// <summary>
        /// Gets or sets a path of working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets the SPICE netlist case-sensitivity settings.
        /// </summary>
        public CaseSensitivitySettings CaseSensitivity { get; }

        /// <summary>
        /// Gets the SPICE netlist parser settings.
        /// </summary>
        public SpiceNetlistParserSettings Parsing { get; }

        /// <summary>
        /// Gets the SPICE netlist model reader settings.
        /// </summary>
        public SpiceNetlistReaderSettings Reading { get; }
    }
}
