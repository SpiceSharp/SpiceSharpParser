namespace SpiceSharpParser.Common
{
    public static class StringExtensions
    {
        public static bool StartsWithCharacterAfterWhitespace(this string @string, char startCharacter)
        {
            if (@string == null)
            {
                return false;
            }

            for (var i = 0; i < @string.Length; i++)
            {
                if (char.IsWhiteSpace(@string[i]))
                {
                    continue;
                }

                return @string[i] == startCharacter;
            }

            return false;
        }
    }
}
