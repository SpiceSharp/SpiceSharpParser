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
        private List<LexerRegexRule> _regexRules = new List<LexerRegexRule>();
        private List<LexerDynamicRule> _dynamicRules = new List<LexerDynamicRule>();

        /// <summary>
        /// Clears the builder.
        /// </summary>
        public void Clear()
        {
            this._regexRules.Clear();
            this._dynamicRules.Clear();
        }

        /// <summary>
        /// Adds a rule to builder.
        /// </summary>
        /// <param name="rule">
        /// A lexer rule.
        /// </param>
        public void AddRegexRule(LexerRegexRule rule)
        {
            this._regexRules.Add(rule);
        }

        /// <summary>
        /// Adds a rule to builder.
        /// </summary>
        /// <param name="rule">
        /// A lexer rule.
        /// </param>
        public void AddDynamicRule(LexerDynamicRule rule)
        {
            this._dynamicRules.Add(rule);
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

            foreach (var lexerTokenRule in this._regexRules.Where(
                rule => rule.GetType().GetTypeInfo().IsGenericType
                && rule.GetType().GetGenericTypeDefinition() == typeof(LexerTokenRule<>)))
            {
                var lexerTokenRuleCloned = lexerTokenRule.Clone() as LexerTokenRule<TLexerState>;

                foreach (var privateTokenRule in this._regexRules.Where(rule => rule is LexerInternalRule))
                {
                    lexerTokenRuleCloned.RegularExpressionPattern = lexerTokenRuleCloned.RegularExpressionPattern.Replace($"<{privateTokenRule.Name}>", privateTokenRule.RegularExpressionPattern);
                }

                tokenRules.Add(lexerTokenRuleCloned);
            }

            return new LexerGrammar<TLexerState>(tokenRules, _dynamicRules);
        }
    }
}
