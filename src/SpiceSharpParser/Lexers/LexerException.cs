using System;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Exception during lexing
    /// </summary>
    public class LexerException : Exception
    {
        public LexerException()
            : base()
        {
        }

        public LexerException(string message)
            : base(message)
        {
        }

        public LexerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
