using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    public class SingleSpiceNetlistParser : ISingleSpiceNetlistParser
    {
        public SingleSpiceNetlistParser(SingleSpiceNetlistParserSettings settings)
        {
            Settings = settings;
        }

        public SingleSpiceNetlistParserSettings Settings { get; set; }

        /// <summary>
        /// Parses a SPICE netlist and returns a SPICE model.
        /// </summary>
        /// <param name="tokens">SPICE netlist tokens.</param>
        /// <returns>
        /// A SPICE netlist model.
        /// </returns>
        public SpiceNetlist Parse(SpiceToken[] tokens)
        {
            ParseTreeNonTerminalNode parseTreeRoot = GetParseTree(
                tokens,
                Settings.HasTitle ? Symbols.Netlist : Symbols.NetlistWithoutTitle,
                Settings.IsNewlineRequired,
                Settings.IsDotStatementCaseSensitive);

            return GetNetlistModelFromTree(parseTreeRoot);
        }

        /// <summary>
        /// Gets the parse tree.
        /// </summary>
        /// <param name="tokens">Array of tokens.</param>
        /// <param name="rootSymbol">Root symbol.</param>
        /// <param name="isNewlineRequiredAtTheEnd">Specifies whether new line character is required at the last token.</param>
        /// <param name="isDotStatementNameCaseSensitive">Specifies whether case is ignored for dot statements.</param>
        /// <returns>
        /// A reference to the root of parse tree.
        /// </returns>
        private ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens, string rootSymbol, bool isNewlineRequiredAtTheEnd, bool isDotStatementNameCaseSensitive)
        {
            var generator = new ParseTreeGenerator(isNewlineRequiredAtTheEnd, isDotStatementNameCaseSensitive);
            return generator.GetParseTree(tokens, rootSymbol);
        }

        /// <summary>
        /// Gets the netlist model.
        /// </summary>
        /// <param name="root">A parse tree root.</param>
        /// <returns>
        /// A netlist model.
        /// </returns>
        private SpiceNetlist GetNetlistModelFromTree(ParseTreeNonTerminalNode root)
        {
            var translator = new ParseTreeEvaluator();
            return translator.Evaluate(root) as SpiceNetlist;
        }
    }
}
