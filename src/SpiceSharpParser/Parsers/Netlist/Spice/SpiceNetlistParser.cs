using System.Linq;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser
{
    public class SpiceNetlistParser : ISpiceNetlistParser
    {
        /// <summary>
        /// Parses a spice netlist and returns a spice model.
        /// </summary>
        /// <param name="spiceNetlist">Spice netlist to parse.</param>
        /// <param name="settings">Setting for parser.</param>
        /// <returns>
        /// A spice netlist model.
        /// </returns>
        public SpiceNetlist Parse(string spiceNetlist, SpiceNetlistParserSettings settings)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            if (spiceNetlist == null)
            {
                throw new System.ArgumentNullException(nameof(spiceNetlist));
            }

            var tokens = GetTokens(spiceNetlist, settings.HasTitle);
            CheckTokens(settings, tokens);

            ParseTreeNonTerminalNode parseTreeRoot = GetParseTree(
                tokens,
                settings.HasTitle ? Symbols.NETLIST : Symbols.NETLIST_WITHOUT_TITLE,
                settings.IsNewlineRequired);

            return GetNetlistModelFromTree(parseTreeRoot);
        }

        /// <summary>
        /// Verifies that tokens are ok.
        /// </summary>
        private static void CheckTokens(SpiceNetlistParserSettings settings, SpiceToken[] tokens)
        {
            if (settings.IsEndRequired)
            {
                if ((tokens.Length >= 2 && tokens[tokens.Length - 2].SpiceTokenType != SpiceTokenType.END
                    && tokens.Length >= 3 && tokens[tokens.Length - 3].SpiceTokenType != SpiceTokenType.END)
                    || (tokens.Length == 1 && tokens[0].SpiceTokenType == SpiceTokenType.EOF))
                {
                    throw new System.Exception("No .END keyword");
                }
            }
        }

        /// <summary>
        /// Gets the tokens.
        /// </summary>
        /// <param name="netlist">Netlist to tokenize.</param>
        /// <param name="hasTitle">Has a title.</param>
        /// <returns>
        /// Array of tokens.
        /// </returns>
        private SpiceToken[] GetTokens(string netlist, bool hasTitle)
        {
            var lexer = new SpiceLexer(new SpiceLexerOptions { HasTitle = hasTitle });
            var tokensEnumerable = lexer.GetTokens(netlist);
            return tokensEnumerable.ToArray();
        }

        /// <summary>
        /// Gets the parse tree.
        /// </summary>
        /// <param name="tokens">Array of tokens.</param>
        /// <param name="rootSymbol">Root symbol.</param>
        /// <returns>
        /// A reference to the root of parse tree.
        /// </returns>
        private ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens, string rootSymbol, bool isNewlineRequiredAtTheEnd)
        {
            return new ParseTreeGenerator(isNewlineRequiredAtTheEnd).GetParseTree(tokens, rootSymbol);
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
