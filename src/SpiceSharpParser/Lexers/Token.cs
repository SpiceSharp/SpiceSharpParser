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
        public Token(int tokenType, string lexem, int lineNumber, string fileName)
        {
            Type = tokenType;
            Lexem = lexem;
            LineNumber = lineNumber;
            FileName = fileName;
        }

        /// <summary>
        /// Gets token type.
        /// </summary>
        public int Type { get; }

        /// <summary>
        /// Gets token line number.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets token file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets token lexem.
        /// </summary>
        public string Lexem { get; set; }
    }
}