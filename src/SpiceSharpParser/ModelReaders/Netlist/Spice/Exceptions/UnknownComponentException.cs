using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    public class UnknownComponentException : ReadingException
    {
        public UnknownComponentException()
        {
        }

        public UnknownComponentException(string message)
            : base(message)
        {
        }

        public UnknownComponentException(string message, int lineNumber)
            : base(message, lineNumber)
        {
        }

        public UnknownComponentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}