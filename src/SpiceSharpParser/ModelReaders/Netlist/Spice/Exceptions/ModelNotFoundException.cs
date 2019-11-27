using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    public class ModelNotFoundException : ReadingException
    {
        public ModelNotFoundException()
        {
        }

        public ModelNotFoundException(string message)
            : base(message)
        {
        }

        public ModelNotFoundException(string message, int line)
            : base(message, line)
        {
        }

        public ModelNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}