using System.Collections.Generic;
using System.Linq;

namespace NLex
{
    /// <summary>
    /// Builds LexerGrammar from lexer rules either of type LexerTokenRole or LexerInternalRule
    /// </summary>
    /// <typeparam name="TLexerState"></typeparam>
    public class LexerGrammarBuilder<TLexerState> where TLexerState: LexerState
    {
        private List<LexerRule> rules = new List<LexerRule>();

        public void Clear()
        {
            rules.Clear();
        }

        public void AddRule(LexerRule rule)
        {
            rules.Add(rule);
        }

        public LexerGrammar<TLexerState> GetGrammar()
        {
            var tokenRules = new List<LexerTokenRule<TLexerState>>();

            foreach (var lexerTokenRule in this.rules.Where(rule => rule.GetType().IsGenericType && rule.GetType().GetGenericTypeDefinition() == typeof(LexerTokenRule<>)))
            {
                var lexerTokenRuleCloned = lexerTokenRule.Clone() as LexerTokenRule<TLexerState>;

                foreach (var privateTokenRule in this.rules.Where(rule => rule is LexerInternalRule))
                {
                    lexerTokenRuleCloned.RegularExpressionPattern = 
                        lexerTokenRuleCloned.RegularExpressionPattern.Replace($"<{privateTokenRule.Name}>", privateTokenRule.RegularExpressionPattern);
                }

                tokenRules.Add(lexerTokenRuleCloned);
            }
            
            return new LexerGrammar<TLexerState>(tokenRules);
        }
    }
}
