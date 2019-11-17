using System.Collections.Generic;
using System.IO;
using System.Text;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Allows fast reading of text lines with line continuation support.
    /// </summary>
    public class LexerStringReader
    {
        private readonly char? _nextLineContinuationCharacter;
        private readonly char? _currentLineContinuationCharacter;
        private readonly char[] _strCharacters = null;
        private int _currentIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerStringReader"/> class.
        /// </summary>
        /// <param name="string">A string to read</param>
        /// <param name="nextLineContinuationCharacter">A line continuation character (in the next line).</param>
        /// <param name="currentLineContinuationCharacter">A line continuation character (in the current line).</param>
        public LexerStringReader(string @string, char? nextLineContinuationCharacter, char? currentLineContinuationCharacter)
        {
            _strCharacters = @string.ToCharArray();
            _nextLineContinuationCharacter = nextLineContinuationCharacter;
            _currentLineContinuationCharacter = currentLineContinuationCharacter;
        }

        /// <summary>
        /// Read next text line with line ending characters.
        /// </summary>
        /// <returns>A text line.</returns>
        public string ReadLine()
        {
            var start = _currentIndex;

            if (_currentIndex == _strCharacters.Length)
            {
                return null;
            }

            while (_currentIndex < _strCharacters.Length && _strCharacters[_currentIndex] != '\n' && _strCharacters[_currentIndex] != '\r')
            {
                _currentIndex++;
            }

            // end of characters
            if (_currentIndex == _strCharacters.Length)
            {
                return GetSubstring(start, _currentIndex - 1);
            }

            // \r\n ending
            if (_currentIndex < (_strCharacters.Length - 1) && (_strCharacters[_currentIndex] == '\r' && _strCharacters[_currentIndex + 1] == '\n'))
            {
                _currentIndex += 2;
                return GetSubstring(start, _currentIndex - 1);
            }

            // \n or \r ending
            _currentIndex++;

            return GetSubstring(start, _currentIndex - 1);
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
        /// Peeks next line with line ending characters. It doesn't update current index.
        /// </summary>
        /// <param name="currentIndex">A index of start index.</param>
        /// <param name="nextLineIndex">A index at the end of peeked line.</param>
        /// <returns>A text line.</returns>
        public string PeekNextLine(int currentIndex, out int nextLineIndex)
        {
            var storedCurrentIndex = _currentIndex;
            _currentIndex = currentIndex;
            var line = ReadLine();
            nextLineIndex = _currentIndex;
            _currentIndex = storedCurrentIndex;
            return line;
        }

        /// <summary>
        /// Reads logical line.
        /// </summary>
        /// <returns>
        /// A logical line that contains physical lines.
        /// </returns>
        public LexerLogicalLine ReadLogicalLine()
        {
            var result = new List<string>();

            string currentLine = ReadLine();
            result.Add(currentLine);

            while (true)
            {
                bool includeNextLine = _currentLineContinuationCharacter.HasValue && currentLine.GetLastCharacter(out _) == _currentLineContinuationCharacter.Value;
                string nextLine;
                // iterate over empty lines
                int start = _currentIndex;
                var emptyLines = new List<string>();
                do
                {
                    nextLine = PeekNextLine(start, out int nextCurrentIndex);

                    if (nextLine.IsEmptyLine())
                    {
                        emptyLines.Add(nextLine);
                    }

                    start = nextCurrentIndex;
                }
                while (nextLine.IsEmptyLine());

                // continuation line  (next line character)
                if (_nextLineContinuationCharacter.HasValue &&
                    nextLine.StartsWithCharacterAfterWhitespace(_nextLineContinuationCharacter.Value))
                {
                    // add empty lines
                    result.AddRange(emptyLines);
                    result.Add(nextLine);
                    _currentIndex = start;
                }
                else if (includeNextLine)
                {
                    result.Add(nextLine);
                    _currentIndex = start;

                    if (nextLine == null)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return new LexerLogicalLine(result);
        }

        private string GetSubstring(int startIndex, int endIndex)
        {
            return new string(_strCharacters, startIndex, endIndex - startIndex + 1);
        }
    }
}
