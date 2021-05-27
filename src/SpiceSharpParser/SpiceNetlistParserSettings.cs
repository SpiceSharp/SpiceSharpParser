using SpiceSharpParser.Common;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist.Spice;
using System.Text;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the SPICE netlist parser.
    /// </summary>
    public class SpiceNetlistParserSettings
    {
        public SpiceNetlistParserSettings()
        {
            CaseSensitivity = new SpiceNetlistCaseSensitivitySettings();
            Lexing = new SpiceLexerSettings(CaseSensitivity);
            Parsing = new SingleSpiceNetlistParserSettings(Lexing);
        }

        /// <summary>
        /// Gets or sets a path of working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets encoding of external files (libs or includes).
        /// </summary>
        public Encoding ExternalFilesEncoding { get; set; } = Encoding.Default;

        /// <summary>
        /// Gets the SPICE netlist lexer settings.
        /// </summary>
        public SpiceLexerSettings Lexing { get; }

        /// <summary>
        /// Gets the SPICE netlist parser settings.
        /// </summary>
        public SingleSpiceNetlistParserSettings Parsing { get; }

        /// <summary>
        /// Gets the case sensitivity settings.
        /// </summary>
        public SpiceNetlistCaseSensitivitySettings CaseSensitivity { get; }
    }
}