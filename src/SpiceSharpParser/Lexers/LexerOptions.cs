namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Options for <see cref="Lexer{TLexerState}"/>.
    /// </summary>
    public class LexerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerOptions"/> class.
        /// </summary>
        /// <param name="multipleLineTokens">Allows multiline tokens.</param>
        /// <param name="nextLineContinuationCharacter">Line continuation character.</param>
        public LexerOptions(bool multipleLineTokens, char? nextLineContinuationCharacter, char? currentLineContinuationCharacter)
        {
            MultipleLineTokens = multipleLineTokens;
            NextLineContinuationCharacter = nextLineContinuationCharacter;
            CurrentLineContinuationCharacter = currentLineContinuationCharacter;

            if (MultipleLineTokens && !NextLineContinuationCharacter.HasValue)
            {
                throw new System.Exception();
            }
        }

        /// <summary>
        /// Gets a value indicating whether token's lexem can be 'multi-line' or 'single-line'.
        /// </summary>
        public bool MultipleLineTokens { get; } = false;

        /// <summary>
        /// Gets the character that makes next line to be part of current line (character is at first posion on second line).
        /// </summary>
        /// <remarks>
        /// In Spice netlist this is '+' character.
        /// </remarks>
        public char? NextLineContinuationCharacter { get; }

        /// <summary>
        /// Gets the character that makes next line to be part of current line ((character is at last posion on current line).
        /// </summary>
        /// <remarks>
        /// For example: '\' character.
        /// </remarks>
        public char? CurrentLineContinuationCharacter { get; }
    }
}
