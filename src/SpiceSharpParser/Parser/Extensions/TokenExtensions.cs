using SpiceSharpParser.Grammar;
using SpiceSharpParser.SpiceLexer;

namespace SpiceSharpParser.Parser.Extensions
{
    /// <summary>
    /// Extensions for <see cref="SpiceToken"/> class.
    /// </summary>
    public static class TokenExtensions
    {
        /// <summary>
        /// Checks whether the spice token is given type
        /// </summary>
        /// <param name="token">A token to check</param>
        /// <param name="type">A given type</param>
        /// <returns>
        /// True if <paramref name="token"/> is given type
        /// </returns>
        public static bool Is(this SpiceToken token, SpiceTokenType type)
        {
            return token.SpiceTokenType == type;
        }

        /// <summary>
        /// Checks whether the spice token has specified lexem
        /// </summary>
        /// <param name="token">A token to check</param>
        /// <param name="lexem">A given lexem </param>
        /// <param name="caseInsensitive">Specified the comparision is case insensitive</param>
        /// <returns>
        /// True if <paramref name="token"/> has specified lexem
        /// </returns>
        public static bool Equal(this SpiceToken token, string lexem, bool caseInsensitive)
        {
            if (caseInsensitive)
            {
                return token.Lexem.ToLower() == lexem.ToLower();
            }

            return token.Lexem == lexem;
        }
    }
}
