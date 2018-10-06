using System;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    public class NoEndKeywordException : Exception
    {
        public NoEndKeywordException()
        {
        }

        public NoEndKeywordException(string message) : base(message)
        {
        }

        public NoEndKeywordException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
