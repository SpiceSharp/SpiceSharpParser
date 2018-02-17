using NLexer;
using SpiceGrammar;
using SpiceLexer;

namespace SpiceParser
{
    public static class TokenExtensions
    {
        public static bool Is(this SpiceToken token, SpiceTokenType type)
        {
            return token.SpiceTokenType == type;
        }

        public static bool Equal(this Token token, string value, bool caseInsensitive)
        {
            if (caseInsensitive)
            {
                return token.Lexem.ToLower() == value.ToLower();
            }

            return token.Lexem == value;
        }
    }
}
