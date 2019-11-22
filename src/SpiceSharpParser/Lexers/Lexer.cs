using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// General lexer. It produces tokens from given text.
    /// </summary>
    /// <typeparam name="TLexerState">Type of lexer state.</typeparam>
    public class Lexer<TLexerState>
        where TLexerState : LexerState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer{TLexerState}"/> class.
        /// </summary>
        /// <param name="grammar">Lexer grammar.</param>
        /// <param name="options">Lexer options.</param>
        public Lexer(LexerGrammar<TLexerState> grammar)
        {
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
        }

        /// <summary>
        /// Gets lexer grammar.
        /// </summary>
        protected LexerGrammar<TLexerState> Grammar { get; }

        /// <summary>
        /// Gets tokens for grammar.
        /// </summary>
        /// <param name="text">A text for which tokens will be returned.</param>
        /// <param name="state">A state for lexer.</param>
        /// <returns>An enumerable of tokens.</returns>
        public IEnumerable<Token> GetTokens(string text, TLexerState state = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            int currentTokenIndex = 0;
            var lineProvider = new LexerLineNumberProvider(text);

            if (state != null)
            {
                state.LineNumber = 0;
            }

            while (currentTokenIndex < text.Length)
            {
                if (state != null)
                {
                    var currentLineIndex = lineProvider.GetLineForIndex(currentTokenIndex);
                    if (state.LineNumber != currentLineIndex)
                    {
                        state.NewLine = true;
                    }
                    else
                    {
                        state.NewLine = false;
                    }

                    state.LineNumber = currentLineIndex;
                }

                if (FindBestTokenRule(text, currentTokenIndex, state, out var bestTokenRule, out var bestMatch))
                {
                    var tokenActionResult = bestTokenRule.ReturnDecisionProvider(state, bestMatch.Value);
                    if (tokenActionResult == LexerRuleReturnDecision.ReturnToken)
                    {
                        if (state != null)
                        {
                            state.PreviousReturnedTokenType = bestTokenRule.TokenType;
                        }

                        yield return new Token(bestTokenRule.TokenType, bestMatch.Value, state?.LineNumber ?? 0);
                    }

                    currentTokenIndex += bestMatch.Length;
                }
                else
                {
                    bool matched = false;
                    var textForDynamicRules = text.Substring(currentTokenIndex);
                    foreach (LexerDynamicRule dynamicRule in Grammar.DynamicRules)
                    {
                        bool ruleMatch = textForDynamicRules.StartsWith(dynamicRule.Prefix);
                        if (ruleMatch)
                        {
                            var dynamicResult = dynamicRule.Action(textForDynamicRules);

                            yield return new Token(dynamicRule.TokenType, dynamicResult.Item1,
                                state?.LineNumber ?? 0);

                            currentTokenIndex += dynamicResult.Item2;
                            matched = true;
                        }
                    }

                    if (!matched)
                    {
                        throw new LexerException($"Can't get next token from text: '{textForDynamicRules}'");
                    }
                }
            }

            // yield EOF token
            yield return new Token(-1, "EOF", state?.LineNumber ?? 0);
        }

        /// <summary>
        /// Finds the best matched <see cref="LexerTokenRule{TLexerState}" /> for remaining text to generate new token.
        /// </summary>
        /// <returns>
        /// True if there is matching <see cref="LexerTokenRule{TLexerState}" />.
        /// </returns>
        private bool FindBestTokenRule(
            string textToLex,
            int startIndex,
            TLexerState state,
            out LexerTokenRule<TLexerState> bestMatchTokenRule,
            out Match bestMatch)
        {
            bestMatchTokenRule = null;
            bestMatch = null;
            foreach (LexerTokenRule<TLexerState> tokenRule in Grammar.RegexRules)
            {
                Match tokenMatch = tokenRule.RegularExpression.Match(textToLex, startIndex, textToLex.Length - startIndex);
                if (tokenMatch.Success && tokenMatch.Length > 0 && tokenMatch.Index == startIndex)
                {
                    state.FullMatch = tokenMatch.Length == (textToLex.Length - startIndex);

                    if (tokenRule.CanUse(state, tokenMatch.Value))
                    {
                        if (bestMatch == null || tokenMatch.Length > bestMatch.Length || tokenRule.TopRule)
                        {
                            bestMatch = tokenMatch;
                            bestMatchTokenRule = tokenRule;

                            if (state.FullMatch || tokenRule.TopRule)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return bestMatch != null;
        }
    }
}