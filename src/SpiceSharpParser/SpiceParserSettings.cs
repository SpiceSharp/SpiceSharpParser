using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the SPICE netlist parser.
    /// </summary>
    public class SpiceParserSettings
    {
        public SpiceParserSettings()
        {
            CaseSensitivity = new CaseSensitivitySettings();
            Parsing = new SingleSpiceNetlistParserSettings(CaseSensitivity);
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
        public SingleSpiceNetlistParserSettings Parsing { get; }

        /// <summary>
        /// Gets the SPICE netlist model reader settings.
        /// </summary>
        public SpiceNetlistReaderSettings Reading { get; }
    }
}
