using SpiceSharpParser.Lexer;

namespace SpiceSharpParser.Lexer.Netlist.Spice
{
    /// <summary>
    /// A token from spice netlist
    /// </summary>
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

        /// <summary>
        /// Gets token line number
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets token type
        /// </summary>
        public SpiceTokenType SpiceTokenType { get; }

        /// <summary>
        /// Updates the line number
        /// </summary>
        /// <param name="lineNumber">New line number</param>
        public void UpdateLineNumber(int lineNumber)
        {
            this.LineNumber = lineNumber;
        }
    }
}
