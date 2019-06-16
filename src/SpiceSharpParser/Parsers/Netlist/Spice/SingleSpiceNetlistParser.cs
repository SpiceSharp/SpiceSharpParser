using System;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist.Spice.Internals;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    public class SingleSpiceNetlistParser : ISingleSpiceNetlistParser
    {
        public SingleSpiceNetlistParser(SingleSpiceNetlistParserSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Gets or sets the parser settings.
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

            VerifyTokens(tokens, Settings.IsEndRequired, Settings.IsNewlineRequired);

            ParseTreeNonTerminalNode parseTreeRoot = GetParseTree(
                tokens,
                Settings.Lexer.HasTitle ? Symbols.Netlist : Symbols.NetlistWithoutTitle,
                Settings.Lexer.IsDotStatementNameCaseSensitive);

            return GetNetlistModelFromTree(parseTreeRoot);
        }

        /// <summary>
        /// Gets the parse tree.
        /// </summary>
        /// <param name="tokens">Array of tokens.</param>
        /// <param name="rootSymbol">Root symbol.</param>
        /// <param name="isDotStatementNameCaseSensitive">Specifies whether case is ignored for dot statements.</param>
        /// <returns>
        /// A reference to the root of parse tree.
        /// </returns>
        private ParseTreeNonTerminalNode GetParseTree(
            SpiceToken[] tokens,
            string rootSymbol,
            bool isDotStatementNameCaseSensitive)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (rootSymbol == null)
            {
                throw new ArgumentNullException(nameof(rootSymbol));
            }

            var generator = new ParseTreeGenerator(isDotStatementNameCaseSensitive);
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

        private void VerifyTokens(SpiceToken[] tokens, bool isEndRequired, bool isNewLineRequired)
        {
            if (isEndRequired)
            {
                // skip EOF and newlines
                int currentIndex = tokens.Length - 2;
                while (tokens[currentIndex].SpiceTokenType == SpiceTokenType.NEWLINE)
                {
                    currentIndex--;
                }

                if (tokens[currentIndex].SpiceTokenType != SpiceTokenType.END)
                {
                    throw new ParseException("No .END dot statement", tokens[currentIndex].LineNumber);
                }
            }

            if (isNewLineRequired)
            {
                if (tokens[tokens.Length - 2].SpiceTokenType != SpiceTokenType.NEWLINE)
                {
                    throw new ParseException("No new line at the end of the netlist", tokens[tokens.Length - 2].LineNumber);
                }
            }
        }
    }
}
