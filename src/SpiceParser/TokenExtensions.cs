using NLex;
using SpiceNetlist;

namespace SpiceParser
{
    public static class TokenExtensions
    {
        public static bool Is(this Token token, SpiceToken type)
        {
            return token.TokenType == (int)type;
        }

        public static bool Equal(this Token token, string value, bool caseInsensitive)
        {
            if (caseInsensitive)
            {
                return token.Value.ToLower() == value.ToLower();
            }
            return token.Value == value;
        }
    }
}
