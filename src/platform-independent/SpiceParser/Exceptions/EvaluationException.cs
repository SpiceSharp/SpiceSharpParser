using System;

namespace SpiceParser.Exceptions
{
    /// <summary>
    /// Exception during evaluating a parse tree
    /// </summary>
    public class EvaluationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationException"/> class.
        /// </summary>
        /// <param name="message">An exception message</param>
        public EvaluationException(string message)
            : base(message)
        {
        }
    }
}
