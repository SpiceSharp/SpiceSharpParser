namespace SpiceSharpParser.Lexer
{
    /// <summary>
    /// A base class for lexer state clasess. It contains a type of previous token
    /// </summary>
    public class LexerState
    {
        /// <summary>
        /// Gets or sets type of previously generated token
        /// </summary>
        public int PreviousTokenType { get; set; } = 0;
    }
}
