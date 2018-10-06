using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Lexer grammar. It contains a collection of lexer token rules.
    /// </summary>
    /// <typeparam name="TLexerState">A type of lexer state.</typeparam>
    public class LexerGrammar<TLexerState>
        where TLexerState : LexerState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerGrammar{TLexerState}"/> class.
        /// </summary>
        /// <param name="lexerRules">A collection of lexer rules.</param>
        public LexerGrammar(ICollection<LexerTokenRule<TLexerState>> lexerRules)
        {
            LexerRules = lexerRules ?? throw new ArgumentException(nameof(lexerRules));
        }

        /// <summary>
        /// Gets lexer token rules.
        /// </summary>
        public ICollection<LexerTokenRule<TLexerState>> LexerRules { get; }
    }
}
