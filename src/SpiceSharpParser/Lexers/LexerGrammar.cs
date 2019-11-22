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
        /// <param name="dynamicRules">A collection of dynamic lexer rules.</param>
        public LexerGrammar(ICollection<LexerTokenRule<TLexerState>> lexerRules, ICollection<LexerDynamicRule> dynamicRules)
        {
            RegexRules = lexerRules ?? throw new ArgumentException(nameof(lexerRules));
            DynamicRules = dynamicRules ?? throw new ArgumentException(nameof(dynamicRules));
        }

        /// <summary>
        /// Gets lexer regex rules.
        /// </summary>
        public ICollection<LexerTokenRule<TLexerState>> RegexRules { get; }

        /// <summary>
        /// Gets lexer dynamic rules.
        /// </summary>
        public ICollection<LexerDynamicRule> DynamicRules { get; private set; }
    }
}