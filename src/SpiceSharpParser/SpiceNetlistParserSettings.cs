using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist.Spice;
using System.Text;
using SpiceSharpParser.Common;

namespace SpiceSharpParser
{
    /// <summary>
    /// Settings for the SPICE netlist parser.
    /// </summary>
    public class SpiceNetlistParserSettings
    {
        public SpiceNetlistParserSettings()
        {
            Lexing = new SpiceLexerSettings();
            Parsing = new SingleSpiceNetlistParserSettings(Lexing);
            CaseSensitivity = new SpiceNetlistCaseSensitivitySettings();
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
        /// Gets or sets the case sensitivity settings.
        /// </summary>
        public ISpiceNetlistCaseSensitivitySettings CaseSensitivity { get; set; }
    }
}