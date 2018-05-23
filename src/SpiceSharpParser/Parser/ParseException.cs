using System;

namespace SpiceSharpParser.Parser
{
    /// <summary>
    /// Exception during creating a parse tree
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">An exception message</param>
        /// <param name="lineNumber">A line number of spice netlist where exception occurs</param>
        public ParseException(string message, int lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Gets the line number of spice netlist where exception occurs
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the string represenation of the exception
        /// </summary>
        /// <returns>
        /// A string
        /// </returns>
        public override string ToString()
        {
            return "Parse exception in line= " + LineNumber + ", message=" + Message;
        }
    }
}
