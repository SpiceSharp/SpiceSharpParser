using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Linq;
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
        public LexerResult GetTokens(string text, TLexerState state = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var result = new LexerResult();

            try
            {
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
                        state.NewLine = state.LineNumber != currentLineIndex;
                        state.LineNumber = currentLineIndex;
                        state.StartColumnIndex = lineProvider.GetColumnForIndex(currentTokenIndex);
                    }

                    if (FindBestRule(text, currentTokenIndex, state, out var tokenType, out var returnToken, out var bestMatch, out var length))
                    {
                        if (returnToken)
                        {
                            if (state != null)
                            {
                                state.PreviousReturnedTokenType = tokenType;
                            }

                            result.Tokens.Add(new Token(
                                (int)tokenType,
                                bestMatch,
                                state?.LineNumber ?? 0,
                                state?.StartColumnIndex ?? 0,
                                null));
                        }

                        currentTokenIndex += length;
                    }
                    else
                    {
                        throw new LexerException(
                            "Can't get next token from text",
                            new SpiceLineInfo
                            {
                                LineNumber = state?.LineNumber ?? 0,
                                StartColumnIndex = state?.StartColumnIndex ?? 0,
                            });
                    }
                }

                result.Tokens.Add(new Token(-1, "EOF", state?.LineNumber ?? 0, state?.StartColumnIndex ?? 0, null));
            }
            catch (LexerException ex)
            {
                result.LexerException = ex;
            }

            return result;
        }

        /// <summary>
        /// Finds the best matched <see cref="LexerTokenRule{TLexerState}" /> for remaining text to generate new token.
        /// </summary>
        /// <returns>
        /// True if there is matching <see cref="LexerTokenRule{TLexerState}" />.
        /// </returns>
        private bool FindBestRule(
            string textToLex,
            int startIndex,
            TLexerState state,
            out int tokenType,
            out bool returnToken,
            out string bestMatch,
            out int bestMatchTotalLength)
        {
            returnToken = false;
            bestMatch = null;
            tokenType = 0;
            bestMatchTotalLength = 0;
            bool skipDynamic = false;
            foreach (LexerTokenRule<TLexerState> tokenRule in Grammar.RegexRules)
            {
                state.CurrentRuleRegex = tokenRule.RegularExpression;

                Match tokenMatch = tokenRule.RegularExpression.Match(textToLex, startIndex, textToLex.Length - startIndex);
                if (tokenMatch.Success && tokenMatch.Length > 0 && tokenMatch.Index == startIndex)
                {
                    state.FullMatch = tokenMatch.Length == (textToLex.Length - startIndex);

                    if (tokenRule.CanUse(state, tokenMatch.Value))
                    {
                        if (bestMatch == null || tokenMatch.Length > bestMatch.Length || tokenRule.TopRule)
                        {
                            bestMatch = tokenMatch.Value;
                            bestMatchTotalLength = bestMatch.Length;
                            returnToken = tokenRule.ReturnDecisionProvider(state, bestMatch) == LexerRuleReturnDecision.ReturnToken;
                            tokenType = tokenRule.TokenType;
                            if (state.FullMatch || tokenRule.TopRule)
                            {
                                skipDynamic = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (!skipDynamic)
            {
                foreach (LexerDynamicRule dynamicRule in Grammar
                    .DynamicRules
                    .Where(rule => rule.PreviousReturnedTokenTypes == null || rule.PreviousReturnedTokenTypes.Contains(state.PreviousReturnedTokenType)))
                {
                    state.CurrentRuleRegex = dynamicRule.PrefixExpression;

                    Match tokenMatch = dynamicRule.PrefixExpression.Match(textToLex, startIndex, textToLex.Length - startIndex);
                    bool ruleMatch = tokenMatch.Success;
                    if (ruleMatch)
                    {
                        var dynamicResult = dynamicRule.Action(textToLex.Substring(startIndex), state);

                        if (bestMatch == null || dynamicResult.Item2 > bestMatch.Length)
                        {
                            bestMatch = dynamicResult.Item1;
                            returnToken = true;
                            tokenType = dynamicRule.TokenType;
                            bestMatchTotalLength = dynamicResult.Item2;
                        }
                    }
                }
            }

            return !string.IsNullOrEmpty(bestMatch);
        }
    }
}