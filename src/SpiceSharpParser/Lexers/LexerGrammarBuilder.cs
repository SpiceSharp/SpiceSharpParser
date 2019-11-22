using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Builder for LexerGrammar object from lexer rules either of type LexerTokenRole or LexerInternalRule.
    /// </summary>
    /// <typeparam name="TLexerState">A type of lexer state.</typeparam>
    public class LexerGrammarBuilder<TLexerState>
        where TLexerState : LexerState
    {
        private readonly List<LexerRegexRule> _regexRules = new List<LexerRegexRule>();
        private readonly List<LexerDynamicRule> _dynamicRules = new List<LexerDynamicRule>();

        /// <summary>
        /// Clears the builder.
        /// </summary>
        public void Clear()
        {
            _regexRules.Clear();
            _dynamicRules.Clear();
        }

        /// <summary>
        /// Adds a rule to builder.
        /// </summary>
        /// <param name="rule">
        /// A lexer rule.
        /// </param>
        public void AddRegexRule(LexerRegexRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            _regexRules.Add(rule);
        }

        /// <summary>
        /// Adds a rule to builder.
        /// </summary>
        /// <param name="rule">
        /// A lexer rule.
        /// </param>
        public void AddDynamicRule(LexerDynamicRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            _dynamicRules.Add(rule);
        }

        /// <summary>
        /// Gets the generated grammar.
        /// </summary>
        /// <returns>
        /// A new grammar that contains rules that were added.
        /// </returns>
        public LexerGrammar<TLexerState> GetGrammar()
        {
            var tokenRules = new List<LexerTokenRule<TLexerState>>();

            var internalTokenRules = _regexRules.Where(rule => rule is LexerInternalRule).ToList();

            foreach (var lexerTokenRule in _regexRules.Where(
                rule => rule.GetType().GetTypeInfo().IsGenericType
                && rule.GetType().GetGenericTypeDefinition() == typeof(LexerTokenRule<>)))
            {
                var lexerTokenRuleCloned = lexerTokenRule.Clone() as LexerTokenRule<TLexerState>;

                if (lexerTokenRuleCloned != null)
                {
                    foreach (var internalTokenRule in internalTokenRules)
                    {
                        lexerTokenRuleCloned.RegularExpressionPattern =
                            lexerTokenRuleCloned.RegularExpressionPattern.Replace(
                                $"<{internalTokenRule.Name}>",
                                internalTokenRule.RegularExpressionPattern);
                    }
                }

                tokenRules.Add(lexerTokenRuleCloned);
            }

            return new LexerGrammar<TLexerState>(tokenRules, _dynamicRules);
        }
    }
}