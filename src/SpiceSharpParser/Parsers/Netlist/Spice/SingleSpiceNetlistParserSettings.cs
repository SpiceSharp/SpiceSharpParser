using SpiceSharpParser.Lexers.Netlist.Spice;
using System;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// Settings for the SPICE netlist parser.
    /// </summary>
    public class SingleSpiceNetlistParserSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleSpiceNetlistParserSettings"/> class.
        /// </summary>
        /// <param name="lexerSettings">Lexer settings.</param>
        public SingleSpiceNetlistParserSettings(SpiceLexerSettings lexerSettings)
        {
            Lexer = lexerSettings ?? throw new ArgumentNullException(nameof(lexerSettings));
        }

        /// <summary>
        /// Gets the lexer settings.
        /// </summary>
        public SpiceLexerSettings Lexer { get; }

        /// <summary>
        /// Gets or sets a value indicating whether .END token is required at the end.
        /// </summary>
        public bool IsEndRequired { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether new line characters are required at the end of the netlist.
        /// </summary>
        public bool IsNewlineRequired { get; set; } = false;
    }
}