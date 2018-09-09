using System;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Allows fast reading of text lines
    /// </summary>
    public class LexerStringReader
    {
        private readonly string str = null;
        private readonly char? nextLineContinuationCharacter;
        private readonly char? currentLineContinuationCharacter;
        private char[] strCharacters = null;
        private int currentIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerStringReader"/> class.
        /// </summary>
        /// <param name="str">A string to read</param>
        /// <param name="nextLineContinuationCharacter">A line continuation character (in the next line)/param>
        /// <param name="currentLineContinuationCharacter">A line continuation character (in the current line)/param>
        public LexerStringReader(string str, char? nextLineContinuationCharacter, char? currentLineContinuationCharacter)
        {
            this.str = str;
            this.nextLineContinuationCharacter = nextLineContinuationCharacter;
            this.currentLineContinuationCharacter = currentLineContinuationCharacter;
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
        /// A continuation line.
        /// </returns>
        public string ReadLineWithContinuation()
        {
            string result = ReadLine();

            while (true)
            {
                int nextCurrentIndex;
                string nextLine = PeekNextLine(out nextCurrentIndex);
                if (nextLine != string.Empty
                    && (nextLine[0] == nextLineContinuationCharacter))
                {
                    currentIndex = nextCurrentIndex;
                    result += nextLine;
                }
                else if (currentLineContinuationCharacter.HasValue && GetLastCharacter(result, out var position) == currentLineContinuationCharacter)
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

        private char? GetLastCharacter(string result, out int position)
        {
            if (result.EndsWith("\r\n") && result.Length >= 3)
            {
                position = result.Length - 3;
                return result[result.Length - 3];
            }

            if (result.EndsWith("\n") && result.Length >= 2)
            {
                position = result.Length - 2;
                return result[result.Length - 2];
            }

            if (result.Length >= 1)
            {
                position = result.Length - 1;
                return result[result.Length - 1];
            }
            else
            {
                position = -1;
                return null;
            }
        }

        /// <summary>
        /// Gets the substring of all text.
        /// </summary>
        /// <param name="startIndex">Start index of the substring.</param>
        /// <returns>
        /// A substring from the text.
        /// </returns>
        public string GetSubstring(int startIndex)
        {
            return new string(strCharacters, startIndex, strCharacters.Length - startIndex);
        }
    }
}
