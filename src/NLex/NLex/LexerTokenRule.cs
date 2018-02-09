using System;

namespace NLex
{
    public class LexerTokenRule<TLexerState> : LexerRule where TLexerState : LexerState
    {
        private Func<TLexerState, LexerRuleUseState> isActive;

        public int TokenType { get; }

        public Func<TLexerState, LexerRuleResult> LexerRuleResultAction { get; private set; }

        public LexerTokenRule(
            int tokenType,
            string name,
            string regularExpressionPattern,
            Func<TLexerState, LexerRuleResult> lexerRuleResultAction = null,
            Func<TLexerState, LexerRuleUseState> isActive = null) : base(name, regularExpressionPattern)
        {
            TokenType = tokenType;
            LexerRuleResultAction = lexerRuleResultAction ?? new Func<TLexerState, LexerRuleResult>((state) => LexerRuleResult.ReturnToken);
            this.isActive = isActive ?? new Func<TLexerState, LexerRuleUseState>((state) => LexerRuleUseState.Use);
        }

        internal override LexerRule Clone()
        {
            return new LexerTokenRule<TLexerState>(this.TokenType, this.Name, this.RegularExpressionPattern, this.LexerRuleResultAction, this.isActive);
        }

        internal bool IsActive(TLexerState lexerState)
        {
            return this.isActive(lexerState) == LexerRuleUseState.Use;
        }
    }
}
