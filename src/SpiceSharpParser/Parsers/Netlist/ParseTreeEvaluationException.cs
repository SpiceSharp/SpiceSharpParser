using System;

namespace SpiceSharpParser.Parsers.Netlist
{
    /// <summary>
    /// Exception during evaluation of a parse tree.
    /// </summary>
    public class ParseTreeEvaluationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeEvaluationException"/> class.
        /// </summary>
        /// <param name="message">An exception message</param>
        public ParseTreeEvaluationException(string message)
            : base(message)
        {
        }
    }
}