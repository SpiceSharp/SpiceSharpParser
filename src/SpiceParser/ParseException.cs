using System;

namespace SpiceParser
{
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }
    }
}
