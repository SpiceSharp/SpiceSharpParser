using System;
using System.Runtime.Serialization;
using SpiceSharpParser.Lexers.Expressions;

namespace SpiceSharpParser.Parsers.Expression
{
    /// <summary>
    /// An exception that is thrown while parsing.
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="message"></param>
        public ParserException(Lexer lexer, string message)
            : base($"{message} at position {lexer.Index - lexer.Length + 1}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ParserException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="info">The message.</param>
        /// <param name="context">The streaming context.</param>
        protected ParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
