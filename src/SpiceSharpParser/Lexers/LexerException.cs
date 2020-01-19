using System;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Exception during lexing.
    /// </summary>
    public class LexerException : SpiceSharpParserException
    {
        public LexerException(string message)
            : base(message)
        {
        }

        public LexerException(string message, SpiceLineInfo lineInfo)
            : base(message, lineInfo)
        {
        }

        public LexerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public LexerException(string message, Exception inner, SpiceLineInfo lineInfo)
            : base(message, inner, lineInfo)
        {
        }
    }
}