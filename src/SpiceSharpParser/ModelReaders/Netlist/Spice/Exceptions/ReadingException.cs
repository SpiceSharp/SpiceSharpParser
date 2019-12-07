using System;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice;

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

        public ReadingException(string message, SpiceLineInfo lineInfo)
            : base(message, lineInfo)
        {
        }

        public ReadingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ReadingException(string message, Exception innerException, SpiceLineInfo lineInfo)
            : base(message, innerException, lineInfo)
        {
        }
    }
}