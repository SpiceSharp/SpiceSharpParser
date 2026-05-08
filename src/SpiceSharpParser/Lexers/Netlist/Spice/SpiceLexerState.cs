namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// <see cref="SpiceLexer"/> state.
    /// </summary>
    public class SpiceLexerState : LexerState
    {
        /// <summary>
        /// Gets or sets a value indicating whether lexer in "comment" lexing state.
        /// </summary>
        public bool InCommentBlock { get; set; }

        /// <summary>
        /// Gets or sets the assignment name before the last equal token.
        /// </summary>
        public string AssignmentNameBeforeEqual { get; set; }
    }
}
