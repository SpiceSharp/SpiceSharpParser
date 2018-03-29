using SpiceSharpParser.Lexer;

namespace SpiceSharpParser.Lexer.Spice3f5
{
    /// <summary>
    /// <see cref="SpiceLexer"/> state
    /// </summary>
    public class SpiceLexerState : LexerState
    {
        /// <summary>
        /// Gets or sets the current line number
        /// </summary>
        public int LineNumber { get; set; } = 1;
    }
}
