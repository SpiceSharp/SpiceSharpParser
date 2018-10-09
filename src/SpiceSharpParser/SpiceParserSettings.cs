using SpiceSharpParser.Common;
using SpiceSharpParser.Lexers.Netlist.Spice;
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
            Lexing = new SpiceLexerSettings();
            Parsing = new SingleSpiceNetlistParserSettings(Lexing);
            Reading = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(Lexing));
        }

        /// <summary>
        /// Gets or sets a path of working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets the SPICE netlist lexer settings.
        /// </summary>
        public SpiceLexerSettings Lexing { get; }

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
