using System;
using System.Linq;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public class SpiceTokenProvider : ISpiceTokenProvider
    {
        public SpiceToken[] GetTokens(string netlist, bool IsDotStatementCaseSensitive, bool hasTitle, bool isEndRequired)
        {
            var tokens = ReadTokens(netlist, hasTitle, IsDotStatementCaseSensitive);
            VerifyTokens(tokens, isEndRequired);
            return tokens;
        }

        /// <summary>
        /// Gets the tokens.
        /// </summary>
        /// <param name="netlist">Netlist to tokenize.</param>
        /// <returns>
        /// Array of tokens.
        /// </returns>
        private SpiceToken[] ReadTokens(string netlist, bool hasTitle, bool IsDotStatementCaseSensitive)
        {
            var lexer = new SpiceLexer(new SpiceLexerOptions { HasTitle = hasTitle, IgnoreCaseDotStatements = !IsDotStatementCaseSensitive });
            var tokensEnumerable = lexer.GetTokens(netlist);
            return tokensEnumerable.ToArray();
        }

        /// <summary>
        /// Verifies that tokens are ok.
        /// </summary>
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
                    throw new NoEndKeywordException();
                }
            }
        }
    }
}