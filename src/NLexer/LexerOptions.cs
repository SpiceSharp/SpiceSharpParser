namespace NLexer
{
    public class LexerOptions
    {
        /// <summary>
        /// Specifies that all tokens' lexems are 'single-line'
        /// </summary>
        public bool SingleLineTokens { get; set; } = true;

        /// <summary>
        /// Specifies that token's lexem can be 'multi-line'
        /// </summary>
        public bool MultipleLineTokens { get; set; } = false;

        /// <summary>
        /// The character that makes next line to be part of current line
        /// </summary>
        /// <remarks>
        /// In Spice netlist this is '+' character
        /// </remarks>
        public char? LineContinuationCharacter { get; set; }
    }
}
