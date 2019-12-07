using System;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    public class WrongParameterTypeException : ReadingException
    {
        public WrongParameterTypeException()
        {
        }

        public WrongParameterTypeException(string message)
            : base(message)
        {
        }

        public WrongParameterTypeException(string message, SpiceLineInfo line)
            : base(message, line)
        {
        }

        public WrongParameterTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}