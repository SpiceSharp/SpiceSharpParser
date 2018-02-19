using System;
using System.Collections.Generic;
using NLexer;
using SpiceGrammar;

namespace SpiceLexer
{
    /// <summary>
    /// A lexer for Spice netlists
    /// </summary>
    public class SpiceLexer
    {
        private LexerGrammar<SpiceLexerState> grammar;
        private SpiceLexerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceLexer"/> class.
        /// </summary>
        /// <param name="options">options for lexer</param>
        public SpiceLexer(SpiceLexerOptions options)
        {
            this.options = options ?? throw new System.ArgumentNullException(nameof(options));
            BuildGrammar();
        }

        /// <summary>
        /// Gets tokens for Spice netlist
        /// </summary>
        /// <param name="netlistText">A string with Spice netlist</param>
        /// <returns>
        /// An enumerable of tokens
        /// </returns>
        public IEnumerable<SpiceToken> GetTokens(string netlistText)
        {
            var state = new SpiceLexerState();
            var lexer = new Lexer<SpiceLexerState>(grammar, new LexerOptions(true, '+'));

            foreach (var token in lexer.GetTokens(netlistText, state))
            {
                yield return new SpiceToken((SpiceTokenType)token.TokenType, token.Lexem, state.LineNumber);
            }
        }

        /// <summary>
        /// Builds Spice lexer grammar
        /// </summary>
        private void BuildGrammar()
        {
            var builder = new LexerGrammarBuilder<SpiceLexerState>();
            builder.AddRule(new LexerInternalRule("LETTER", "[a-z]"));
            builder.AddRule(new LexerInternalRule("CHARACTER", "[a-z0-9]"));
            builder.AddRule(new LexerInternalRule("DIGIT", "[0-9]"));
            builder.AddRule(new LexerInternalRule("SPECIAL", "[_\\.\\:\\!%\\#\\-;\\<>\\^]"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WHITESPACE,
                "A whitespace characters that will be ignored",
                "[ \t]*",
                (SpiceLexerState state) =>
                {
                    return LexerRuleResult.IgnoreToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.ASTERIKS,
                "An asteriks character",
                "\\*"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.PLUS,
                "A plus character",
                "\\+"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.MINUS,
                "A minus character",
                "-"));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DOT,
                    "A dot character",
                    "\\."));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.COMMA,
                    "A comma character",
                    ","));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DELIMITER,
                    "A delimeter character",
                    @"(\(|\)|\|)"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.EQUAL,
                "An equal character",
                @"="));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.NEWLINE,
                "A new line characters",
                @"(\r\n|\n|\r)",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.ReturnToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.CONTINUE,
                "A continuation token",
                @"((\r\n\+|\n\+|\r\+))",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.IgnoreToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.ENDS,
                ".ends keyword",
                ".ends"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.END,
                ".end keyword",
                ".end"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.VALUE,
                "A value",
                @"(([+-])?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+|[tgmkunpf](<LETTER>)*)?)"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.COMMENT,
                "A comment (without asterix)",
                "[^\r\n]+",
                null,
                (SpiceLexerState state) =>
                {
                    if (state.PreviousTokenType != (int)SpecialTokenType.Unknown && state.PreviousTokenType == (int)SpiceTokenType.ASTERIKS)
                    {
                        return LexerRuleUseState.Use;
                    }
                    return LexerRuleUseState.Skip;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.TITLE,
                "The title - first line of spice token",
                "[^\r\n]+",
                null,
                (SpiceLexerState state) =>
                {
                    if (state.LineNumber == 1 && options.HasTitle)
                    {
                        return LexerRuleUseState.Use;
                    }
                    return LexerRuleUseState.Skip;
                }));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.STRING,
                    "A string with quotation marks",
                    @"""[^\r\n]+\"""));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.EXPRESSION,
                "A mathematical expression",
                "{[^{}]*}"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.REFERENCE,
                "A reference",
                "@<WORD>"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WORD,
                "A word",
                "(<LETTER>(<CHARACTER>|<SPECIAL>)*)"));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.IDENTIFIER,
                    "An identifier",
                    "((<CHARACTER>|_)(<CHARACTER>|<SPECIAL>)*)"));

            grammar = builder.GetGrammar();
        }
    }
}
