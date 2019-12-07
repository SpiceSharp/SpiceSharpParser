using System;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    public class WrongParameterException : ReadingException
    {
        public WrongParameterException()
        {
        }

        public WrongParameterException(string message)
            : base(message)
        {
        }

        public WrongParameterException(string message, SpiceLineInfo line)
            : base(message, line)
        {
        }

        public WrongParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}