using System;
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

        /// <summary>
        /// Gets the parser settings.
        /// </summary>
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
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            VerifyTokens(tokens, Settings.IsEndRequired);

            ParseTreeNonTerminalNode parseTreeRoot = GetParseTree(
                tokens,
                Settings.Lexer.HasTitle ? Symbols.Netlist : Symbols.NetlistWithoutTitle,
                Settings.IsNewlineRequired,
                Settings.Lexer.IsDotStatementNameCaseSensitive);

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
        private ParseTreeNonTerminalNode GetParseTree(
            SpiceToken[] tokens,
            string rootSymbol, 
            bool isNewlineRequiredAtTheEnd, 
            bool isDotStatementNameCaseSensitive)
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

        private void VerifyTokens(SpiceToken[] tokens, bool isEndRequired)
        {
            if (isEndRequired)
            {
                if ((tokens.Length >= 2
                     && tokens[tokens.Length - 2].TokenType != (int)SpiceTokenType.END
                     && tokens.Length >= 3
                     && tokens[tokens.Length - 3].TokenType != (int)SpiceTokenType.END)
                    || (tokens.Length == 1 && tokens[0].TokenType == (int)SpiceTokenType.EOF))
                {
                    throw new Exception("No .END dot statement");
                }
            }
        }
    }
}
