using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Lexers
{
    public class LexerLineNumberProvider
    {
        private readonly string _text;
        private readonly List<LexerLineRange> _ranges = new List<LexerLineRange>();

        public LexerLineNumberProvider(string text)
        {
            _text = text;

            Init();
        }

        public int GetLineForIndex(int index)
        {
            foreach (var t in _ranges)
            {
                if (t.From <= index && index <= t.To)
                {
                    return t.LineNumber;
                }
            }

            return _ranges.Last().LineNumber;
        }

        private void Init()
        {
            int currentIndex = 0;

            var newRange = new LexerLineRange {From = 0, LineNumber = 1};
            int currentLineNumber = 1;

            while (currentIndex < _text.Length)
            {
                if (_text[currentIndex] == '\r')
                {
                    if ((_text.Length > currentIndex + 1) && _text[currentIndex + 1] == '\n')
                    {
                        newRange.To = currentIndex + 1;
                        newRange.LineNumber = currentLineNumber;
                        _ranges.Add(newRange);

                        currentLineNumber++;
                        currentIndex += 2;

                        newRange = new LexerLineRange {From = currentIndex, LineNumber = currentLineNumber};
                    }
                    else
                    {
                        newRange.To = currentIndex;
                        newRange.LineNumber = currentLineNumber;
                        _ranges.Add(newRange);

                        currentLineNumber++;
                        currentIndex++;

                        newRange = new LexerLineRange {From = currentIndex, LineNumber = currentLineNumber};
                    }
                }
                else if (_text[currentIndex] == '\n')
                {
                    newRange.To = currentIndex;
                    newRange.LineNumber = currentLineNumber;
                    _ranges.Add(newRange);

                    currentLineNumber++;
                    currentIndex++;

                    newRange = new LexerLineRange {From = currentIndex, LineNumber = currentLineNumber};
                }
                else
                {
                    currentIndex++;
                }
            }

            newRange.To = _text.Length - 1;
            _ranges.Add(newRange);
        }
    }
}
