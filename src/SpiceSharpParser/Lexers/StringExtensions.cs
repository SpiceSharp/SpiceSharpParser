namespace SpiceSharpParser.Lexers
{
    public static class StringExtensions
    {
        public static bool IsEmptyLine(this string line)
        {
            return line == string.Empty || line == "\r" || line == "\n" || line == "\r\n";
        }

        public static char? GetLastCharacter(this string line, out int position)
        {
            position = -1;
            if (line == null) return null;

            if (line.EndsWith("\r\n") && line.Length >= 3)
            {
                position = line.Length - 3;
                return line[line.Length - 3];
            }

            if (line.EndsWith("\n") && line.Length >= 2)
            {
                position = line.Length - 2;
                return line[line.Length - 2];
            }

            if (line.Length >= 1)
            {
                position = line.Length - 1;
                return line[line.Length - 1];
            }
            else
            {
                position = -1;
                return null;
            }
        }
    }
}