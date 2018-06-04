using System;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// The lexer token rule class. It defines how and when a token will be generated for given regulal expression pattern
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
        /// <param name="returnAction">A token rule token action</param>
        /// <param name="isActiveAction">A token rule active action</param>
        /// <param name="ignoreCase">Ignore case</param>
        public LexerTokenRule(
            int tokenType,
            string ruleName,
            string regularExpressionPattern,
            Func<TLexerState, string, LexerRuleResult> returnAction = null,
            Func<TLexerState, LexerRuleUseState> isActiveAction = null,
            bool ignoreCase = true)
            : base(ruleName, regularExpressionPattern, ignoreCase)
        {
            TokenType = tokenType;
            ReturnAction = returnAction ?? new Func<TLexerState, string, LexerRuleResult>((state, lexem) => LexerRuleResult.ReturnToken);
            IsActiveAction = isActiveAction ?? new Func<TLexerState, LexerRuleUseState>((state) => LexerRuleUseState.Use);
        }

        /// <summary>
        ///  Gets the type of a generated token.
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Gets the function to execute before returning token.
        /// </summary>
        public Func<TLexerState, string, LexerRuleResult> ReturnAction { get; }

        /// <summary>
        /// Gets whether the rule should be skipped.
        /// </summary>
        protected Func<TLexerState, LexerRuleUseState> IsActiveAction { get; }

        /// <summary>
        /// Returns true if the rule is active or should be skipped.
        /// </summary>
        /// <param name="lexerState">The curent lexer state</param>
        /// <returns>
        /// True if the lexer token rule is active or should be skipped
        /// </returns>
        public bool IsActive(TLexerState lexerState)
        {
            return IsActiveAction(lexerState) == LexerRuleUseState.Use;
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
                IsActiveAction,
                IgnoreCase);
        }
    }
}
