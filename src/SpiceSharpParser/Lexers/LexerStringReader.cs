using System;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Allows fast reading of text lines with line continuation support.
    /// </summary>
    public class LexerStringReader
    {
        private readonly char? _nextLineContinuationCharacter;
        private readonly char? _currentLineContinuationCharacter;
        private readonly char[] strCharacters = null;
        private int _currentIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerStringReader"/> class.
        /// </summary>
        /// <param name="string">A string to read</param>
        /// <param name="nextLineContinuationCharacter">A line continuation character (in the next line).</param>
        /// <param name="currentLineContinuationCharacter">A line continuation character (in the current line).</param>
        public LexerStringReader(string @string, char? nextLineContinuationCharacter, char? currentLineContinuationCharacter)
        {
            _nextLineContinuationCharacter = nextLineContinuationCharacter;
            _currentLineContinuationCharacter = currentLineContinuationCharacter;
            strCharacters = @string.ToCharArray();
        }

        /// <summary>
        /// Read next text line with line ending characters.
        /// </summary>
        /// <returns>A text line</returns>
        public string ReadLine()
        {
            var start = _currentIndex;

            if (_currentIndex > (strCharacters.Length - 1))
            {
                return string.Empty;
            }

            while (_currentIndex < (strCharacters.Length - 1)
                && strCharacters[_currentIndex] != '\n'
                && strCharacters[_currentIndex] != '\r')
            {
                _currentIndex++;
            }

            if (_currentIndex < (strCharacters.Length - 1))
            {
                if (strCharacters[_currentIndex] == '\r' && strCharacters[_currentIndex + 1] == '\n')
                {
                    _currentIndex++;
                }
            }

            var line = new string(strCharacters, start, _currentIndex - start + 1);
            _currentIndex++;

            return line;
        }

        /// <summary>
        /// Peeks next line with line ending characters. It doesn't update current index.
        /// </summary>
        /// <param name="nextLineIndex">A index at the end of peeked line.</param>
        /// <returns>A text line.</returns>
        public string PeekNextLine(out int nextLineIndex)
        {
            var storedCurrentIndex = _currentIndex;
            var line = ReadLine();
            nextLineIndex = _currentIndex;
            _currentIndex = storedCurrentIndex;
            return line;
        }

        /// <summary>
        /// Reads next continuation line.
        /// </summary>
        /// <returns>
        /// A continuation line.
        /// </returns>
        public string ReadLineWithContinuation(out int continuationLines)
        {
            string result = ReadLine();
            continuationLines = 0;

            while (true)
            {
                string nextLine = PeekNextLine(out int nextCurrentIndex);
                if (nextLine != string.Empty && nextLine.TrimStart(' ', '\t').StartsWith(_nextLineContinuationCharacter.ToString()))
                {
                    continuationLines++;
                    _currentIndex = nextCurrentIndex;
                    result = result.TrimEnd('\r', '\n');
                    result += $" {nextLine.Trim(' ', '\t').Substring(1)}"; // skip _nextLineContinuationCharacter
                }
                else if (_currentLineContinuationCharacter.HasValue && GetLastCharacter(result, out var position) == _currentLineContinuationCharacter)
                {
                    _currentIndex = nextCurrentIndex;
                    result = result.Remove(position, 1).TrimEnd('\r', '\n');  // skip _currentLineContinuationCharacter
                    result += $" {nextLine}";
                }
                else
                {
                    break;
                }
            }

            return result;
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
    }
}
