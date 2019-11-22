using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    /// <summary>
    /// Unknown component exception.
    /// </summary>
    public class UnknownComponentException : Exception
    {
        public UnknownComponentException()
        {
        }

        public UnknownComponentException(string message)
            : base(message)
        {
        }

        public UnknownComponentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}