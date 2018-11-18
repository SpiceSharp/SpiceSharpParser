using SpiceSharpParser.Lexers;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// <see cref="SpiceLexer"/> state.
    /// </summary>
    public class SpiceLexerState : LexerState
    {
        public SpiceLexerState()
        {
        }
      
        /// <summary>
        /// Gets or sets a value indicating whether lexer in "comment" lexing state.
        /// </summary>
        public bool InCommentBlock { get; set; }

        /// <summary>
        /// Gets or sets the lexer options.
        /// </summary>
        public LexerOptions LexerOptions { get; set; }
    }
}
