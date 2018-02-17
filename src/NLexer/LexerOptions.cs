namespace NLexer
{
    public class LexerOptions
    {
        /// <summary>
        /// Specifies that all tokens' lexems are 'single-line'
        /// </summary>
        public bool SingleLineTokens { get; } = true;

        /// <summary>
        /// Specifies that token's lexem can be 'multi-line'
        /// </summary>
        public bool MultipleLineTokens { get; } = false;

        /// <summary>
        /// The character that makes next line to be part of current line
        /// </summary>
        /// <remarks>
        /// In Spice netlist this is '+' character
        /// </remarks>
        public char? LineContinuationCharacter { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="singleLineTokens"></param>
        /// <param name="multipleLineTokens"></param>
        /// <param name="lineContinuationCharacter"></param>
        public LexerOptions(bool singleLineTokens, bool multipleLineTokens, char? lineContinuationCharacter)
        {
            SingleLineTokens = singleLineTokens;
            MultipleLineTokens = multipleLineTokens;
            LineContinuationCharacter = lineContinuationCharacter;

            if (SingleLineTokens && MultipleLineTokens)
            {
                throw new System.Exception();
            }

            if (MultipleLineTokens && !LineContinuationCharacter.HasValue)
            {
                throw new System.Exception();
            }
        }
    }
}
