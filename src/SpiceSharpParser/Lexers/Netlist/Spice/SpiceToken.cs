namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// A token from SPICE netlist.
    /// </summary>
    public class SpiceToken : Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceToken"/> class.
        /// </summary>
        public SpiceToken(SpiceTokenType tokenType, string lexem, int lineNumber = 0, int startColumnIndex = 0, string fileName = null)
            : base((int)tokenType, lexem, lineNumber, startColumnIndex, fileName)
        {
            SpiceTokenType = tokenType;
        }

        /// <summary>
        /// Gets token type.
        /// </summary>
        public SpiceTokenType SpiceTokenType { get; }
    }
}