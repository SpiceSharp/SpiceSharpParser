using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Lexers
{
    public class LexerLogicalLine
    {
        private readonly IList<string> _physicalLines;

        public LexerLogicalLine(IList<string> physicalLines)
        {
            _physicalLines = physicalLines;
        }

        public int PhysicalLinesCount => _physicalLines.Count;

        public string GetLine()
        {
            var result = string.Join(" ",
                _physicalLines.Where(line => line != null && !line.IsEmptyLine())
                    .Select(line => line.TrimEnd('\r', '\n')));

            var lastLine = _physicalLines.LastOrDefault(line => line != null);

            if (lastLine == null)
            {
                return null;
            }

            if (lastLine.EndsWith("\r\n"))
            {
                return $"{result}\r\n";
            }
            else
            {
                if (lastLine.EndsWith("\r"))
                {
                    return $"{result}\r";
                }
            }

            return $"{result}\n";
        }
    }
}
