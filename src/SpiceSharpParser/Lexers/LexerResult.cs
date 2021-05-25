using System.Collections.Generic;

namespace SpiceSharpParser.Lexers
{
    public class LexerResult
    {
        public LexerResult()
        {
            Tokens = new List<Token>();
        }

        public List<Token> Tokens { get; }

        public LexerException LexerException { get; set; }

        public bool IsValid => LexerException == null;
    }
}
