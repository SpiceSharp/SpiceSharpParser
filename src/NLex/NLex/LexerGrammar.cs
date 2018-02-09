using System;
using System.Collections.Generic;

namespace NLex
{
    /// <summary>
    /// Lexer grammar. It contains a collection of lexer token rules
    /// </summary>
    /// <typeparam name="TLexerState"></typeparam>
    public class LexerGrammar<TLexerState> where TLexerState: LexerState
    {
        public ICollection<LexerTokenRule<TLexerState>> LexerRules { get; private set; }

        public LexerGrammar(ICollection<LexerTokenRule<TLexerState>> lexerRules)
        {
            if (lexerRules == null) throw new ArgumentException(nameof(lexerRules));

            LexerRules = lexerRules;
        }
    }
}
