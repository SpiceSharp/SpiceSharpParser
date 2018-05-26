using System;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions
{
    /// <summary>
    /// General reader exception
    /// </summary>
    public class GeneralReaderException : Exception
    {
        public GeneralReaderException()
        {
        }

        public GeneralReaderException(string message)
            : base(message)
        {
        }

        public GeneralReaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
