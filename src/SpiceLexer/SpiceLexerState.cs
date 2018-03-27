using NLexer;

namespace SpiceLexer
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
