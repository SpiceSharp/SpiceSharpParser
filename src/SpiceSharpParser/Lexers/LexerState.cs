namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// A base class for lexer state classes. It contains a type of previous token.
    /// </summary>
    public class LexerState
    {
        /// <summary>
        /// Gets or sets type of previously returned token by lexer.
        /// </summary>
        public int PreviousReturnedTokenType { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether lexem is a full match.
        /// </summary>
        public bool FullMatch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lexem is before a line break character.
        /// </summary>
        public bool BeforeLineBreak { get; set; }
    }
}
