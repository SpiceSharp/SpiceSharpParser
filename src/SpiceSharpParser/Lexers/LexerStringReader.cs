namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Allows fast reading of text lines
    /// </summary>
    public class LexerStringReader
    {
        private readonly string str = null;
        private readonly char? continuationCharacter;
        private char[] strCharacters = null;
        private int currentIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerStringReader"/> class.
        /// </summary>
        /// <param name="str">A string to read</param>
        /// <param name="continuationCharacter">A line continuation character</param>
        public LexerStringReader(string str, char? continuationCharacter)
        {
            this.str = str;
            this.continuationCharacter = continuationCharacter;
            this.strCharacters = str.ToCharArray();
        }

        /// <summary>
        /// Read next text line with line ending characters
        /// </summary>
        /// <returns>A text line</returns>
        public string ReadLine()
        {
            var start = currentIndex;

            if (currentIndex > (strCharacters.Length - 1))
            {
                return string.Empty;
            }

            while (currentIndex < (strCharacters.Length - 1)
                && strCharacters[currentIndex] != '\n'
                && strCharacters[currentIndex] != '\r')
            {
                currentIndex++;
            }

            if (currentIndex < (strCharacters.Length - 1))
            {
                if (strCharacters[currentIndex] == '\r' && strCharacters[currentIndex + 1] == '\n')
                {
                    currentIndex++;
                }
            }

            var line = new string(strCharacters, start, currentIndex - start + 1);
            currentIndex++;

            return line;
        }

        /// <summary>
        /// Peeks next line with line ending characters. It doesn't update current index.
        /// </summary>
        /// <param name="nextLineIndex">A index at the end of peeked line</param>
        /// <returns>A text line</returns>
        public string PeekNextLine(out int nextLineIndex)
        {
            var storedCurrentIndex = currentIndex;
            var line = ReadLine();
            nextLineIndex = currentIndex;
            currentIndex = storedCurrentIndex;
            return line;
        }

        /// <summary>
        /// Reads next continuation line.
        /// </summary>
        /// <returns>
        /// A continuation line
        /// </returns>
        public string ReadLineWithContinuation()
        {
            string result = ReadLine();

            while (true)
            {
                int nextCurrentIndex;
                string nextLine = PeekNextLine(out nextCurrentIndex);
                if (nextLine != string.Empty && nextLine[0] == continuationCharacter)
                {
                    currentIndex = nextCurrentIndex;
                    result += nextLine;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the substring of all text
        /// </summary>
        /// <param name="startIndex">Start index of the substrign</param>
        /// <returns>
        /// A string
        /// </returns>
        public string GetSubstring(int startIndex)
        {
            return new string(strCharacters, startIndex, strCharacters.Length - startIndex);
        }
    }
}
