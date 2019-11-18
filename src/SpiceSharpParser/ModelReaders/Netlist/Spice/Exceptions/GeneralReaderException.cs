using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions
{
    /// <summary>
    /// General reader exception.
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

        public int LineNumber { get; set; }
    }
}
