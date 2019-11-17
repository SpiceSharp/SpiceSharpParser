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

        public string GetLine()
        {
            var result = string.Join("", _physicalLines.Where(line => line != null).Select(line => line));

            var lastLine = _physicalLines.LastOrDefault(line => line != null);

            if (lastLine == null)
            {
                return null;
            }

            return result;
        }
    }
}
