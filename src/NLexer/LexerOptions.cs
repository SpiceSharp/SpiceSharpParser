namespace NLexer
{
    public class LexerOptions
    {
        public bool SingleLineTokens { get; set; } = false;

        public bool MultipleLineTokens { get; set; } = false;

        public char? LineContinuationCharacter { get; set; } 
    }
}
