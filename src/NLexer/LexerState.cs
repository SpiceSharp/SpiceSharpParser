namespace NLexer
{
    /// <summary>
    /// A base class for lexer state clasess. It contains a type of previous token
    /// </summary>
    public class LexerState
    {
        public int? PreviousTokenType { get; set; }
    }
}
