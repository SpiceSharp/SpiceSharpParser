using NLex;

namespace SpiceLex
{
    public class SpiceLexerState : LexerState
    {
        public int LineNumber { get; set; } = 1;
    }
}
