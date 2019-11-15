namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// A token produces by <see cref="Lexer{TLexerState}"/>.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        public Token(int tokenType, string lexem)
        {
            Type = tokenType;
            Lexem = lexem;
        }

        /// <summary>
        /// Gets token type.
        /// </summary>
        public int Type { get; }

        /// <summary>
        /// Gets token lexem.
        /// </summary>
        public string Lexem { get; set; }
    }
}
