using NLex;

namespace SpiceSharpLex
{
    public class SpiceLexerState : LexerState
    {
        public int LineNumber { get; set; } = 1;
    }
}
