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
        public Lexer(LexerGrammar<TLexerState> grammar, LexerOptions options)
        {
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets lexer grammar.
        /// </summary>
        protected LexerGrammar<TLexerState> Grammar { get; }

        /// <summary>
        /// Gets lexer options.
        /// </summary>
        protected LexerOptions Options { get; }

        /// <summary>
        /// Gets tokens for grammar.
        /// </summary>
        /// <param name="text">A text for which tokens will be returned.</param>
        /// <param name="state">A state for lexer.</param>
        /// <returns>An enumerable of tokens.</returns>
        public IEnumerable<Token> GetTokens(string text, TLexerState state = null)
        {
            // TODO: Refactor this please
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            string textToLex = null;
            bool getNextTextToLex = true;
            int currentTokenIndex = 0;

            LexerStringReader strReader = new LexerStringReader(
                text,
                Options.NextLineContinuationCharacter,
                Options.CurrentLineContinuationCharacter);

            var lineProvider = new LexerLineNumberProvider(text);

            if (state != null)
            {
                state.NewLine = true;
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

                    state.LineNumber = currentLineIndex;
                }

                if (getNextTextToLex)
                {
                    textToLex = GetTextToLex(strReader);
                    if (state != null)
                    {
                        state.NewLine = true;
                    }
                    getNextTextToLex = false;
                }

                if (textToLex == null)
                {
                    break;
                }

                if (FindBestTokenRule(textToLex, state, out var bestTokenRule, out var bestMatch))
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

                    UpdateTextToLex(ref textToLex, ref getNextTextToLex, bestMatch.Length, state);
                }
                else
                {
                    bool matched = false;
                    foreach (LexerDynamicRule dynamicRule in Grammar.DynamicRules)
                    {
                        bool ruleMatch = textToLex.StartsWith(dynamicRule.Prefix);
                        if (ruleMatch)
                        {
                            var dynamicResult = dynamicRule.Action(textToLex);

                            yield return new Token(dynamicRule.TokenType, dynamicResult.Item1, state?.LineNumber ?? 0);

                            currentTokenIndex += dynamicResult.Item2;
                            UpdateTextToLex(ref textToLex, ref getNextTextToLex, dynamicResult.Item2, state);
                            matched = true;
                        }
                    }

                    if (!matched)
                    {
                        throw new LexerException("Can't get next token from text: '" + textToLex + "'");
                    }
                }
            }

            // yield EOF token
            yield return new Token(-1, "EOF", state?.LineNumber ?? 0);
        }

        /// <summary>
        /// Updates a text to lex by skipping characters which are part of a generated token.
        /// </summary>
        private void UpdateTextToLex(ref string textToLex, ref bool getNextTextToLex, int tokenLength, TLexerState state = null)
        {
            textToLex = textToLex.Substring(tokenLength);

            if (state != null)
            {
                state.NewLine = false;
            }

            if (string.IsNullOrEmpty(textToLex))
            {
                getNextTextToLex = true;
            }
        }

        /// <summary>
        /// Gets a text from which the tokens will be generated.
        /// </summary>
        private string GetTextToLex(LexerStringReader strReader)
        {
            if (Options.MultipleLineTokens)
            {
                var logicalLine = strReader.ReadLogicalLine();
                var result = logicalLine.GetLine();
                return result;
            }
            else
            {
                var line = strReader.ReadLine();
                return line;
            }
        }

        /// <summary>
        /// Finds the best matched <see cref="LexerTokenRule{TLexerState}" /> for remaining text to generate new token.
        /// </summary>
        /// <returns>
        /// True if there is matching <see cref="LexerTokenRule{TLexerState}" />.
        /// </returns>
        private bool FindBestTokenRule(string remainingText, TLexerState state, out LexerTokenRule<TLexerState> bestMatchTokenRule, out Match bestMatch)
        {
            bestMatchTokenRule = null;
            bestMatch = null;
            foreach (LexerTokenRule<TLexerState> tokenRule  in Grammar.RegexRules)
            {
                Match tokenMatch = tokenRule.RegularExpression.Match(remainingText);
                if (tokenMatch.Success && tokenMatch.Length > 0)
                {
                    state.FullMatch = tokenMatch.Length == remainingText.Length;
                    state.BeforeLineBreak = StartsWithLineBreak(remainingText.Substring(tokenMatch.Length));

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

        private bool StartsWithLineBreak(string v)
        {
            return v.StartsWith("\n", StringComparison.Ordinal) || v.StartsWith("\r\n", StringComparison.Ordinal);
        }
    }
}
