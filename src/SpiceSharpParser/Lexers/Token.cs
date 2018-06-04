namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// A token produces by <see cref="Lexer{TLexerState}"/>
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        public Token(int tokenType, string lexem)
        {
            TokenType = tokenType;
            Lexem = lexem;
        }

        /// <summary>
        /// Gets token type
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Gets token lexem
        /// </summary>
        public string Lexem { get; private set; }

        /// <summary>
        /// Gets th length of lexem
        /// </summary>
        public int Length
        {
            get
            {
                if (Lexem != null)
                {
                    return Lexem.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Updates <see cref="Token"/>'s lexem
        /// </summary>
        /// <param name="lexem">A new value of lexem</param>
        public void UpdateLexem(string lexem)
        {
            this.Lexem = lexem;
        }
    }
}
