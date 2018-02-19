using System;

namespace NLexer
{
    /// <summary>
    /// The lexer token rule class. It defines how and when a token will be generated for given regulal expression pattern
    /// </summary>
    /// <typeparam name="TLexerState"></typeparam>
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
        public LexerTokenRule(
            int tokenType,
            string ruleName,
            string regularExpressionPattern,
            Func<TLexerState, LexerRuleResult> lexerRuleResultAction = null,
            Func<TLexerState, LexerRuleUseState> isActiveAction = null)
            : base(ruleName, regularExpressionPattern)
        {
            TokenType = tokenType;
            LexerRuleResultAction = lexerRuleResultAction ?? new Func<TLexerState, LexerRuleResult>((state) => LexerRuleResult.ReturnToken);
            IsActiveAction = isActiveAction ?? new Func<TLexerState, LexerRuleUseState>((state) => LexerRuleUseState.Use);
        }

        /// <summary>
        /// The type of a generated token
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Specifies what to do with a generated token (return or ignore)
        /// </summary>
        public Func<TLexerState, LexerRuleResult> LexerRuleResultAction { get; }

        /// <summary>
        /// Specifies whether the rule should be skipped
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
                this.TokenType,
                this.Name,
                this.RegularExpressionPattern,
                this.LexerRuleResultAction,
                this.IsActiveAction);
        }
    }
}
