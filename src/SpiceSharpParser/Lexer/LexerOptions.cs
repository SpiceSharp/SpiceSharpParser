namespace SpiceSharpParser.Lexer
{
    /// <summary>
    /// Options for <see cref="Lexer{TLexerState}"/>
    /// </summary>
    public class LexerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerOptions"/> class.
        /// </summary>
        /// <param name="multipleLineTokens">Allows multiline tokens</param>
        /// <param name="lineContinuationCharacter">Line continuation character</param>
        public LexerOptions(bool multipleLineTokens, char? lineContinuationCharacter)
        {
            MultipleLineTokens = multipleLineTokens;
            LineContinuationCharacter = lineContinuationCharacter;

            if (MultipleLineTokens && !LineContinuationCharacter.HasValue)
            {
                throw new System.Exception();
            }
        }

        /// <summary>
        /// Gets a value indicating whether token's lexem can be 'multi-line' or 'single-line'
        /// </summary>
        public bool MultipleLineTokens { get; } = false;

        /// <summary>
        /// Gets the character that makes next line to be part of current line
        /// </summary>
        /// <remarks>
        /// In Spice netlist this is '+' character
        /// </remarks>
        public char? LineContinuationCharacter { get; }
    }
}
