using SpiceSharpParser.Lexer;

namespace SpiceSharpParser.Lexer.Spice
{
    /// <summary>
    /// <see cref="SpiceLexer"/> state
    /// </summary>
    public class SpiceLexerState : LexerState
    {
        public SpiceLexerState()
        {

        }

        /// <summary>
        /// Gets or sets the current line number
        /// </summary>
        public int LineNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether lexer in "comment" lexing state.
        /// </summary>
        public bool InCommentBlock { get; set; }
    }
}
