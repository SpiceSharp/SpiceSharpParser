using System.Linq;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public class SpiceTokenProvider : ISpiceTokenProvider
    {
        public SpiceToken[] GetTokens(string netlist, SpiceLexerSettings options)
        {
            var tokens = ReadTokens(netlist, options.HasTitle, options.IsDotStatementNameCaseSensitive);
            return tokens;
        }

        /// <summary>
        /// Gets the tokens.
        /// </summary>
        /// <param name="netlist">Netlist to tokenize.</param>
        /// <param name="hasTitle">Indicates whether netlist has a title.</param>
        /// <param name="isDotStatementCaseSensitive">Indicates whether dot statement name is case-sensitive.</param>
        /// <returns>
        /// Array of tokens.
        /// </returns>
        private SpiceToken[] ReadTokens(string netlist, bool hasTitle, bool isDotStatementCaseSensitive)
        {
            var lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = hasTitle, IsDotStatementNameCaseSensitive = isDotStatementCaseSensitive });
            var tokensEnumerable = lexer.GetTokens(netlist);
            return tokensEnumerable.ToArray();
        }
    }
}