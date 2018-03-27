using System;

namespace SpiceSharpParser.Lexer
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
        /// <param name="lexerRuleResultAction">A token rule token action</param>
        /// <param name="isActiveAction">A token rule active action</param>
        /// <param name="ignoreCase">Ignore case</param>
        public LexerTokenRule(
            int tokenType,
            string ruleName,
            string regularExpressionPattern,
            Func<TLexerState, LexerRuleResult> lexerRuleResultAction = null,
            Func<TLexerState, LexerRuleUseState> isActiveAction = null,
            bool ignoreCase = true)
            : base(ruleName, regularExpressionPattern, ignoreCase)
        {
            TokenType = tokenType;
            LexerRuleResultAction = lexerRuleResultAction ?? new Func<TLexerState, LexerRuleResult>((state) => LexerRuleResult.ReturnToken);
            IsActiveAction = isActiveAction ?? new Func<TLexerState, LexerRuleUseState>((state) => LexerRuleUseState.Use);
        }

        /// <summary>
        ///  Gets the type of a generated token
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Gets specifies what to do with a generated token (return or ignore)
        /// </summary>
        public Func<TLexerState, LexerRuleResult> LexerRuleResultAction { get; }

        /// <summary>
        /// Gets specifies whether the rule should be skipped
        /// </summary>
        protected Func<TLexerState, LexerRuleUseState> IsActiveAction { get; }

        /// <summary>
        /// Returns true if the rule is active or should be skipped
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
        /// Clones the rule
        /// </summary>
        /// <returns>
        /// A new instance of rule
        /// </returns>
        public override LexerRule Clone()
        {
            return new LexerTokenRule<TLexerState>(
                TokenType,
                Name,
                RegularExpressionPattern,
                LexerRuleResultAction,
                IsActiveAction,
                IgnoreCase);
        }
    }
}
