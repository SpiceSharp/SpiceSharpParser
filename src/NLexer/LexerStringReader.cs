using System;

namespace NLexer
{
    class LexerStringReader
    {
        private readonly string str = null;
        private readonly char? continuationCharacter;
        private char[] strCharacters = null;
        private int currentIndex = 0;

        public LexerStringReader(string str, char? continuationCharacter)
        {
            this.str = str;
            this.continuationCharacter = continuationCharacter;
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

            while (currentIndex < (strCharacters.Length - 1) 
                && strCharacters[currentIndex] != '\n' 
                && strCharacters[currentIndex] != '\r')
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

        public string PeekNextLine(out int nextLineIndex)
        {
            var storedCurrentIndex = currentIndex;
            var line = ReadLine();
            nextLineIndex = currentIndex;
            currentIndex = storedCurrentIndex;
            return line;
        }

        public string ReadLineWithContinuation()
        {
            string result = ReadLine();

            while (true)
            {
                int nextCurrentIndex;
                string nextLine = PeekNextLine(out nextCurrentIndex);
                if (nextLine != "" && nextLine[0] == continuationCharacter)
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

        public string GetSubstring(int startIndex)
        {
            return new string(strCharacters, startIndex, strCharacters.Length - startIndex);
        }
    }
}
