using System;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// The lexer token rule class. It defines how and when a token will be generated for given regulal expression pattern.
    /// </summary>
    /// <typeparam name="TLexerState">Type of lexer state</typeparam>
    public class LexerTokenRule<TLexerState> : LexerRule
        where TLexerState : LexerState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerTokenRule{TLexerState}"/> class.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="ruleName">Rule name</param>
        /// <param name="regularExpressionPattern">A token rule pattern</param>
        /// <param name="returnStateAction">A token rule token action</param>
        /// <param name="handleStateAction">A token rule active action</param>
        /// <param name="ignoreCase">Ignore case</param>
        public LexerTokenRule(
            int tokenType,
            string ruleName,
            string regularExpressionPattern,
            Func<TLexerState, string, LexerRuleReturnState> returnStateAction = null,
            Func<TLexerState, string, LexerRuleHandleState> handleStateAction = null,
            bool ignoreCase = true)
            : base(ruleName, regularExpressionPattern, ignoreCase)
        {
            TokenType = tokenType;
            ReturnAction = returnStateAction ?? new Func<TLexerState, string, LexerRuleReturnState>((state, lexem) => LexerRuleReturnState.ReturnToken);
            UseAction = handleStateAction ?? new Func<TLexerState, string, LexerRuleHandleState>((state, lexem) => LexerRuleHandleState.Use);
        }

        /// <summary>
        ///  Gets the type of a generated token.
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Gets the function to execute before returning token.
        /// </summary>
        public Func<TLexerState, string, LexerRuleReturnState> ReturnAction { get; }

        /// <summary>
        /// Gets whether the rule should be skipped.
        /// </summary>
        protected Func<TLexerState, string, LexerRuleHandleState> UseAction { get; }

        /// <summary>
        /// Returns true if the rule is active or should be skipped.
        /// </summary>
        /// <param name="lexerState">The curent lexer state.</param>
        /// <returns>
        /// True if the lexer token rule is active or should be skipped.
        /// </returns>
        public bool CanUse(TLexerState lexerState, string lexem)
        {
            return UseAction(lexerState, lexem) == LexerRuleHandleState.Use;
        }

        /// <summary>
        /// Clones the rule.
        /// </summary>
        /// <returns>
        /// A clone of rule.
        /// </returns>
        public override LexerRule Clone()
        {
            return new LexerTokenRule<TLexerState>(
                TokenType,
                Name,
                RegularExpressionPattern,
                ReturnAction,
                UseAction,
                IgnoreCase);
        }
    }
}
