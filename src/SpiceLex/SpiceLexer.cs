using System.Collections.Generic;
using NLexer;
using SpiceNetlist;

namespace SpiceLex
{
    /// <summary>
    /// A lexer for Spice netlists
    /// </summary>
    public class SpiceLexer
    {
        private LexerGrammar<SpiceLexerState> grammar;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceLexer"/> class.
        /// </summary>
        public SpiceLexer()
        {
            var builder = new LexerGrammarBuilder<SpiceLexerState>();

            builder.AddRule(new LexerInternalRule("LETTER", "[a-z]"));
            builder.AddRule(new LexerInternalRule("CHARACTER", "[a-z0-9]"));
            builder.AddRule(new LexerInternalRule("DIGIT", "[0-9]"));
            builder.AddRule(new LexerInternalRule("SPECIAL", "[_,\\.\\:\\!%\\#\\-;\\<>\\^]"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.WHITESPACE,
                "A whitespace characters that will be ignored",
                "[ \t]*",
                (SpiceLexerState state) =>
                {
                    return LexerRuleResult.IgnoreToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.ASTERIKS,
                "An asteriks character",
                "\\*"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.PLUS,
                "A plus character",
                "\\+"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.MINUS,
                "A minus character",
                "-"));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceToken.DOT,
                    "A dot character",
                    "\\."));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceToken.COMMA,
                    "A comma character",
                    ","));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceToken.DELIMITER,
                    "A delimeter character",
                    @"(\(|\)|\|)"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.EQUAL,
                "An equal character",
                @"="));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.NEWLINE,
                "A new line characters",
                @"(\r\n|\n|\r)",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.ReturnToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.CONTINUE,
                "A continuation token",
                @"((\r\n\+|\n\+|\r\+))",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.IgnoreToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.ENDS,
                ".ends keyword",
                ".ends"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.END,
                ".end keyword",
                ".end"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.VALUE,
                "A value",
                @"(([+-])?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+|[tgmkunpf](<LETTER>)*)?[vH]?)"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.COMMENT,
                "A comment (without asterix)",
                "[^\r\n]+",
                null,
                (SpiceLexerState state) =>
                {
                    if (state.PreviousTokenType.HasValue && state.PreviousTokenType.Value == (int)SpiceToken.ASTERIKS)
                    {
                        return LexerRuleUseState.Use;
                    }
                    return LexerRuleUseState.Skip;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.TITLE,
                "The title - first line of spice token",
                "[^\r\n]+",
                null,
                (SpiceLexerState state) =>
                {
                    if (state.LineNumber == 1)
                    {
                        return LexerRuleUseState.Use;
                    }
                    return LexerRuleUseState.Skip;
                }));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceToken.STRING,
                    "A string with quotation marks",
                    @"""[^\r\n]+\"""));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.EXPRESSION,
                "A mathematical expression",
                "{[^{}]*}"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.REFERENCE,
                "A reference",
                "@<WORD>"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceToken.WORD,
                "A word",
                "(<LETTER>(<CHARACTER>|<SPECIAL>)*)"));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceToken.IDENTIFIER,
                    "An identifier",
                    "((<CHARACTER>|_)(<CHARACTER>|<SPECIAL>)*)"));

            this.grammar = builder.GetGrammar();
        }

        /// <summary>
        /// Gets tokens for Spice netlist
        /// </summary>
        /// <param name="netlistText">A string with Spice netlist</param>
        /// <returns>
        /// An enumerable of tokens
        /// </returns>
        public IEnumerable<Token> GetTokens(string netlistText)
        {
            var state = new SpiceLexerState();
            var lexer = new Lexer<SpiceLexerState>(this.grammar, new LexerOptions() { MultipleLineTokens = true, LineContinuationCharacter = '+' });
            return lexer.GetTokens(netlistText, state);
        }
    }
}
