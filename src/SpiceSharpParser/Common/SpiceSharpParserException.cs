using System;

namespace SpiceSharpParser.Common
{
    public class SpiceSharpParserException : Exception
    {
        public SpiceSharpParserException()
        {
        }

        public SpiceSharpParserException(string message)
            : base(message)
        {
        }

        public SpiceSharpParserException(string message, int lineNumber)
            : base($"{message} (at line {lineNumber})")
        {
        }

        public SpiceSharpParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SpiceSharpParserException(string message, Exception innerException, int lineNumber)
            : base($"{message} (at line {lineNumber})", innerException)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }
    }
}
