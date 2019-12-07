using System;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    public class WrongParametersCountException : ReadingException
    {
        public WrongParametersCountException()
        {
        }

        public WrongParametersCountException(string message)
            : base(message)
        {
        }

        public WrongParametersCountException(string message, SpiceLineInfo line)
            : base(message, line)
        {
        }

        public WrongParametersCountException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}