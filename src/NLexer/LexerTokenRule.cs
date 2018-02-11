using System;

namespace NLexer
{
    public class LexerTokenRule<TLexerState> : LexerRule
        where TLexerState : LexerState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerTokenRule{TLexerState}"/> class.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="name">Token name</param>
        /// <param name="regularExpressionPattern">A token rule pattern</param>
        /// <param name="lexerRuleResultAction">A token rule token action</param>
        /// <param name="isActiveAction">a token rule active action</param>
        public LexerTokenRule(
            int tokenType,
            string name,
            string regularExpressionPattern,
            Func<TLexerState, LexerRuleResult> lexerRuleResultAction = null,
            Func<TLexerState, LexerRuleUseState> isActiveAction = null)
            : base(name, regularExpressionPattern)
        {
            TokenType = tokenType;
            LexerRuleResultAction = lexerRuleResultAction ?? new Func<TLexerState, LexerRuleResult>((state) => LexerRuleResult.ReturnToken);
            IsActiveAction = isActiveAction ?? new Func<TLexerState, LexerRuleUseState>((state) => LexerRuleUseState.Use);
        }

        public int TokenType { get; }

        public Func<TLexerState, LexerRuleResult> LexerRuleResultAction { get; private set; }

        protected Func<TLexerState, LexerRuleUseState> IsActiveAction { get; set; }

        internal bool IsActive(TLexerState lexerState)
        {
            return IsActiveAction(lexerState) == LexerRuleUseState.Use;
        }

        internal override LexerRule Clone()
        {
            return new LexerTokenRule<TLexerState>(this.TokenType, this.Name, this.RegularExpressionPattern, this.LexerRuleResultAction, this.IsActiveAction);
        }
    }
}
