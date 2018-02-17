using System;

namespace SpiceParser
{
    public class ParseException : Exception
    {
        public ParseException(string message, int lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }

        public override string ToString()
        {
            return "Parse exception in line= " + LineNumber + ", message=" + Message;
        }
    }
}
