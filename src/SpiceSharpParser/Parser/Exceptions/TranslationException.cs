using System;

namespace SpiceSharpParser.Parser.Exceptions
{
    /// <summary>
    /// Exception during translating a parse tree
    /// </summary>
    public class TranslationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationException"/> class.
        /// </summary>
        /// <param name="message">An exception message</param>
        public TranslationException(string message)
            : base(message)
        {
        }
    }
}
