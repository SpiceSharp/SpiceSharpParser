using System.Linq;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public class SpiceTokenProvider : ISpiceTokenProvider
    {
        private readonly SpiceLexer _lexer;

        public SpiceTokenProvider(bool hasTitle, bool isDotStatementCaseSensitive, bool enableBusSyntax)
        {
            _lexer = new SpiceLexer(new SpiceLexerSettings(isDotStatementCaseSensitive)
            {
                HasTitle = hasTitle,
                EnableBusSyntax = enableBusSyntax,
            });
        }

        public SpiceToken[] GetTokens(string netlist)
        {
            var tokensEnumerable = _lexer.GetTokens(netlist);
            return tokensEnumerable.ToArray();
        }
    }
}