using NLexer;

namespace SpiceLexer
{
    public class SpiceLexerState : LexerState
    {
        public int LineNumber { get; set; } = 1;
    }
}
