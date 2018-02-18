using System;
using NLexer;
using SpiceGrammar;

namespace SpiceLexer
{
    public class SpiceToken : Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceToken"/> class.
        /// </summary>
        public SpiceToken(SpiceTokenType tokenType, string lexem, int lineNumber = 0)
            : base((int)tokenType, lexem)
        {
            LineNumber = lineNumber;
            SpiceTokenType = tokenType;
        }

        public int LineNumber { get; private set; }

        public SpiceTokenType SpiceTokenType { get; }

        public void UpdateLineNumber(int lineNumber)
        {
            this.LineNumber = lineNumber;
        }
    }
}
