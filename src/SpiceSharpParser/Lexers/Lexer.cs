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
            Options = options;
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
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
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            LexerTokenRule<TLexerState> bestTokenRule = null;
            Match bestMatch = null;
            string textToLex = null;
            bool getNextTextToLex = true;
            int currentTokenIndex = 0;

            LexerStringReader strReader = new LexerStringReader(
                text,
                Options.NextLineContinuationCharacter,
                Options.CurrentLineContinuationCharacter);

            while (currentTokenIndex < text.Length)
            {
                if (getNextTextToLex)
                {
                    textToLex = GetTextToLex(strReader, currentTokenIndex);
                    getNextTextToLex = false;
                }

                if (string.IsNullOrEmpty(textToLex))
                {
                    break;
                }

                if (FindBestTokenRule(textToLex, state, out bestTokenRule, out bestMatch))
                {
                    var tokenActionResult = bestTokenRule.ReturnDecisionProvider(state, bestMatch.Value);
                    if (tokenActionResult == LexerRuleReturnDecision.ReturnToken)
                    {
                        yield return new Token(bestTokenRule.TokenType, bestMatch.Value);
                        state.PreviousReturnedTokenType = bestTokenRule.TokenType;
                    }

                    currentTokenIndex += bestMatch.Length;

                    UpdateTextToLex(ref textToLex, ref getNextTextToLex, bestMatch.Length);
                }
                else
                {
                    throw new LexerException("Can't get next token from text: '" + textToLex + "'");
                }
            }

            // yield EOF token
            yield return new Token((int)TokenType.EOF, "EOF");
        }

        /// <summary>
        /// Updates a text to lex by skipping characters which are part of a generated token.
        /// </summary>
        private void UpdateTextToLex(ref string textToLex, ref bool getNextTextToLex, int tokenLength)
        {
            textToLex = textToLex.Substring(tokenLength);

            if (string.IsNullOrEmpty(textToLex))
            {
                getNextTextToLex = true;
            }
        }

        /// <summary>
        /// Gets a text from which the tokens will be generated.
        /// </summary>
        private string GetTextToLex(LexerStringReader strReader, int currentTokenIndex)
        {
            if (Options.MultipleLineTokens == false)
            {
                return strReader.ReadLine();
            }
            else if (Options.MultipleLineTokens)
            {
                return strReader.ReadLineWithContinuation();
            }
            else
            {
                return strReader.GetSubstring(currentTokenIndex);
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
            foreach (LexerTokenRule<TLexerState> tokenRule in Grammar.LexerRules)
            {
                Match tokenMatch = tokenRule.RegularExpression.Match(remainingText);
                if (tokenMatch.Success && tokenMatch.Length > 0)
                {
                    state.FullMatch = tokenMatch.Length == remainingText.Length;
                    state.BeforeLineBreak = StartsWithLineBreak(remainingText.Substring(tokenMatch.Length));

                    if (tokenRule.CanUse(state, tokenMatch.Value))
                    {
                        if (bestMatch == null || tokenMatch.Length > bestMatch.Length)
                        {
                            bestMatch = tokenMatch;
                            bestMatchTokenRule = tokenRule;
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
