using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    public class ReadingException : SpiceSharpParserException
    {
        public ReadingException()
        {
        }

        public ReadingException(string message)
            : base(message)
        {
        }

        public ReadingException(string message, int lineNumber)
            : base($"{message} at line {lineNumber}")
        {
        }

        public ReadingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ReadingException(string message, Exception innerException, int lineNumber)
            : base(message, innerException, lineNumber)
        {
        }
    }
}