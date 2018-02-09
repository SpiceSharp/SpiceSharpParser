namespace NLex
{
    class LexerStringReader
    {
        private readonly string str = null;
        private char[] strCharacters = null;
        private int currentIndex = 0;

        public LexerStringReader(string str)
        {
            this.str = str;
            this.strCharacters = str.ToCharArray();
        }

        /// <summary>
        /// Read a text line with line ending characters
        /// </summary>
        /// <returns>A text line</returns>
        public string ReadLine()
        {
            var start = currentIndex;

            if (currentIndex >= (strCharacters.Length - 1)) return "";

            while (currentIndex < (strCharacters.Length - 1) && strCharacters[currentIndex] != '\n' && strCharacters[currentIndex] != '\r')
            {
                currentIndex++;
            }

            if (currentIndex < (strCharacters.Length - 2))
            {
                if (strCharacters[currentIndex]  == '\r' && strCharacters[currentIndex+1] == '\n')
                {
                    currentIndex++;
                }
            }

            var line = new string(strCharacters, start, currentIndex - start + 1);
            currentIndex++;

            return line;
        }
    }
}
