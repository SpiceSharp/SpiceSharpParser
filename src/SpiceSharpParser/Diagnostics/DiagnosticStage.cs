namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Identifies the compilation stage that produced a diagnostic.
    /// </summary>
    public enum DiagnosticStage
    {
        /// <summary>
        /// Tokenization of source text.
        /// </summary>
        Lexer,

        /// <summary>
        /// Parsing tokens into the input model.
        /// </summary>
        Parser,

        /// <summary>
        /// Include, library, macro, and other model preprocessing.
        /// </summary>
        Preprocessor,

        /// <summary>
        /// Translation into SpiceSharp entities and simulations.
        /// </summary>
        Reader,

        /// <summary>
        /// Structural validation of the translated model.
        /// </summary>
        Linter,
    }
}
