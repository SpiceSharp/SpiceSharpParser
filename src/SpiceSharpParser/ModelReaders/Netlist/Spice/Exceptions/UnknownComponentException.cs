using System;
using SpiceSharpParser.Models.Netlist.Spice;

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

        public UnknownComponentException(string message, SpiceLineInfo lineInfo)
            : base(message, lineInfo)
        {
        }

        public UnknownComponentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}