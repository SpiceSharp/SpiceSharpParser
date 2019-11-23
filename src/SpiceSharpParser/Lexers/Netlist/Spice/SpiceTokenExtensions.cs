using System;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// Extensions for <see cref="SpiceToken"/> class.
    /// </summary>
    public static class SpiceTokenExtensions
    {
        /// <summary>
        /// Checks whether the SPICE token is given type.
        /// </summary>
        /// <param name="token">A token to check.</param>
        /// <param name="type">A given type.</param>
        /// <returns>
        /// True if <paramref name="token"/> is given type.
        /// </returns>
        public static bool Is(this SpiceToken token, SpiceTokenType type)
        {
            return token.SpiceTokenType == type;
        }

        /// <summary>
        /// Checks whether the SPICE token has specified lexem.
        /// </summary>
        /// <param name="token">A token to check.</param>
        /// <param name="lexem">A given lexem.</param>
        /// <param name="caseSensitive">Is lexem case sensitive.</param>
        /// <returns>
        /// True if <paramref name="token"/> has specified lexem.
        /// </returns>
        public static bool Equal(this SpiceToken token, string lexem, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                return string.Equals(token.Lexem, lexem, StringComparison.CurrentCultureIgnoreCase);
            }

            return token.Lexem == lexem;
        }
    }
}