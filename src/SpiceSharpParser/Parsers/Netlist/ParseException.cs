using SpiceSharpParser.Common;

namespace SpiceSharpParser.Parsers.Netlist
{
    /// <summary>
    /// Exception during creating a parse tree.
    /// </summary>
    public class ParseException : SpiceSharpParserException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">An exception message.</param>
        /// <param name="lineNumber">A line number of SPICE netlist where exception occurs.</param>
        public ParseException(string message, int lineNumber)
            : base($"{message} at line {lineNumber}")
        {
        }
    }
}