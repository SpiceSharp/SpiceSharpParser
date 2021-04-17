using System;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// The lexer token rule class. It defines how and when a token will be generated for given regular expression pattern.
    /// </summary>
    /// <typeparam name="TLexerState">Type of lexer state.</typeparam>
    public class LexerTokenRule<TLexerState> : LexerRegexRule
        where TLexerState : LexerState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerTokenRule{TLexerState}"/> class.
        /// </summary>
        /// <param name="tokenType">Token type.</param>
        /// <param name="ruleName">Rule name.</param>
        /// <param name="regularExpressionPattern">A token rule pattern.</param>
        /// <param name="returnDecisionProvider">A token rule return decision provider.</param>
        /// <param name="useDecisionProvider">A token rule use decision provider.</param>
        /// <param name="ignoreCase">Ignore case.</param>
        /// <param name="topRule">Is most important rule.</param>
        public LexerTokenRule(
            int tokenType,
            string ruleName,
            string regularExpressionPattern,
            Func<TLexerState, string, LexerRuleReturnDecision> returnDecisionProvider = null,
            Func<TLexerState, string, LexerRuleUseDecision> useDecisionProvider = null,
            bool ignoreCase = true,
            bool topRule = false)
            : base(ruleName, regularExpressionPattern, ignoreCase)
        {
            TokenType = tokenType;
            ReturnDecisionProvider = returnDecisionProvider ?? ((state, lexem) => LexerRuleReturnDecision.ReturnToken);
            UseDecisionProvider = useDecisionProvider ?? ((state, lexem) => LexerRuleUseDecision.Use);
            TopRule = topRule;
        }

        /// <summary>
        ///  Gets the type of a generated token.
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Gets the value indicating whether rule is most important.
        /// </summary>
        public bool TopRule { get; }

        /// <summary>
        /// Gets the provider that tells whether the rule should be returned.
        /// </summary>
        public Func<TLexerState, string, LexerRuleReturnDecision> ReturnDecisionProvider { get; }

        /// <summary>
        /// Gets the provider that tells whether the rule should be used.
        /// </summary>
        public Func<TLexerState, string, LexerRuleUseDecision> UseDecisionProvider { get; }

        /// <summary>
        /// Returns true if the rule is active or should be skipped.
        /// </summary>
        /// <param name="lexerState">The current lexer state.</param>
        /// <param name="lexem">A lexem value.</param>
        /// <returns>
        /// True if the lexer token rule is active or should be skipped.
        /// </returns>
        public bool CanUse(TLexerState lexerState, string lexem)
        {
            return UseDecisionProvider(lexerState, lexem) == LexerRuleUseDecision.Use;
        }

        /// <summary>
        /// Clones the rule.
        /// </summary>
        /// <returns>
        /// A clone of rule.
        /// </returns>
        public override LexerRegexRule Clone()
        {
            return new LexerTokenRule<TLexerState>(
                TokenType,
                Name,
                RegularExpressionPattern,
                ReturnDecisionProvider,
                UseDecisionProvider,
                IgnoreCase,
                TopRule);
        }
    }
}