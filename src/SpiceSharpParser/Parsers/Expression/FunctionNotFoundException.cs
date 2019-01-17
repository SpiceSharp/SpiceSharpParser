using System;

namespace SpiceSharpParser.Parsers.Expression
{
    public class FunctionNotFoundException : Exception
    {
        public FunctionNotFoundException()
        {
        }

        public FunctionNotFoundException(string message)
            : base(message)
        {
        }

        public FunctionNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
