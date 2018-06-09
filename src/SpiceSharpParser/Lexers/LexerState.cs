namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// A base class for lexer state clasess. It contains a type of previous token.
    /// </summary>
    public class LexerState
    {
        /// <summary>
        /// Gets or sets type of previously returned token by lexer.
        /// </summary>
        public int PreviousReturnedTokenType { get; set; } = 0;
    }
}
