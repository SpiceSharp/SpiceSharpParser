using System.Linq;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public class SpiceTokenProvider : ISpiceTokenProvider
    {
        private readonly SpiceLexer _lexer;

        public SpiceTokenProvider(bool hasTitle, bool isDotStatementCaseSensitive)
        {
            _lexer = new SpiceLexer(new SpiceLexerSettings
            {
                HasTitle = hasTitle,
                IsDotStatementNameCaseSensitive = isDotStatementCaseSensitive,
            });
        }

        public SpiceToken[] GetTokens(string netlist)
        {
            var tokensEnumerable = _lexer.GetTokens(netlist);
            return tokensEnumerable.ToArray();
        }
    }
}