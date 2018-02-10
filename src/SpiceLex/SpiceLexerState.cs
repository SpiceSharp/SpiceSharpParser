using NLexer;

namespace SpiceLex
{
    public class SpiceLexerState : LexerState
    {
        public int LineNumber { get; set; } = 1;
    }
}
