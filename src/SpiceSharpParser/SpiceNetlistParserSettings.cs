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
        private int _maximumSyntaxErrors = 25;

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
        /// Gets or sets dialect compatibility options.
        /// </summary>
        public CompatibilityOptions Compatibility { get; set; } = CompatibilityOptions.None;

        /// <summary>
        /// Gets or sets a value indicating whether lexing and parsing should continue at the next
        /// recoverable statement after a syntax error. The default is false for compatibility.
        /// </summary>
        public bool ContinueAfterErrors { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of lexer and parser errors collected before recovery stops.
        /// </summary>
        public int MaximumSyntaxErrors
        {
            get => _maximumSyntaxErrors;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value), "MaximumSyntaxErrors must be positive.");
                }

                _maximumSyntaxErrors = value;
            }
        }

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
