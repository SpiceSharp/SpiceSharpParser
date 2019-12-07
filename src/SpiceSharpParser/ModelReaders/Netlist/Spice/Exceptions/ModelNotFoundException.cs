using System;
using SpiceSharpParser.Models.Netlist.Spice;

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

        public ModelNotFoundException(string message, SpiceLineInfo line)
            : base(message, line)
        {
        }

        public ModelNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}