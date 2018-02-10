using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLexer
{
    public class Lexer<TLexerState> where TLexerState : LexerState
    {
        protected LexerGrammar<TLexerState> Grammar { get; }
        protected LexerOptions Options { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grammar">Lexer grammar</param>
        /// <param name="options">Lexer options</param>
        public Lexer(LexerGrammar<TLexerState> grammar, LexerOptions options)
        {
            Options = options;
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
        }

        /// <summary>
        /// Get tokens for grammar
        /// </summary>
        /// <param name="text">A text for which tokens will be returned</param>
        /// <param name="state">A state for lexer</param>
        /// <returns></returns>
        public IEnumerable<Token> GetTokens(string text, TLexerState state = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            LexerTokenRule<TLexerState> bestTokenRule = null;
            Match bestMatch = null;
            string textToLex = null;
            int currentTokenIndex = 0;

            LexerStringReader strReader = new LexerStringReader(text, Options.LineContinuationCharacter);

            while (currentTokenIndex < text.Length)
            {
                GetNextTextToLex(strReader, currentTokenIndex, ref textToLex);
                if (textToLex == "") break;

                if (FindBestTokenRule(textToLex, state, out bestTokenRule, out bestMatch))
                {
                    var tokenActionResult = bestTokenRule.LexerRuleResultAction(state);
                    if (tokenActionResult == LexerRuleResult.ReturnToken)
                    {
                        yield return new Token()
                        {
                            Value = bestMatch.Value,
                            TokenType = bestTokenRule.TokenType,
                        };
                    }
                    state.PreviousTokenType = bestTokenRule.TokenType;
                    currentTokenIndex += bestMatch.Length;

                    UpdateTextToLex(ref textToLex, bestMatch);

                    if (textToLex == "") break;
                }
                else 
                {
                    // undefined token in text
                    throw new LexerException("Can't get next token from text: '" + textToLex + "'");
                }
            }

            // yield EOF token
            yield return new Token()
            {
                TokenType = -1
            };
        }

        void UpdateTextToLex(ref string textToLex, Match bestMatch)
        {
            if (Options.SingleLineTokens || Options.MultipleLineTokens)
            {
                textToLex = textToLex.Substring(bestMatch.Length);
            }

            if (textToLex == "")
            {
                textToLex = null;
            }
        }

        void GetNextTextToLex(LexerStringReader strReader, int currentTokenIndex, ref string textToLex)
        {
            if (Options.SingleLineTokens)
            {
                if (textToLex == null)
                {
                    textToLex = strReader.ReadLine();
                }
            }
            if (Options.MultipleLineTokens)
            {
                if (textToLex == null)
                {
                    textToLex = strReader.ReadLineWithContinuation();
                }
            }
            else
            {
                textToLex = strReader.GetSubstring(currentTokenIndex);
            }
        }

        bool FindBestTokenRule(string remainingText, TLexerState state, out LexerTokenRule<TLexerState> bestMatchTokenRule, out Match bestMatch)
        {
            bestMatchTokenRule = null;
            bestMatch = null;
            foreach (LexerTokenRule<TLexerState> tokenRule in Grammar.LexerRules)
            {
                if (tokenRule.IsActive(state))
                {
                    Match tokenMatch = tokenRule.RegularExpression.Match(remainingText);
                    if (tokenMatch.Success && tokenMatch.Length > 0)
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
    }
}
