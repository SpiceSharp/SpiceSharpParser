using NLex;
using SpiceNetlist;
using System.Collections.Generic;

namespace SpiceLex
{
    public class SpiceLexer
    {
        LexerGrammar<SpiceLexerState> grammar;

        public SpiceLexer()
        {
            var builder = new LexerGrammarBuilder<SpiceLexerState>();

            builder.AddRule(new LexerInternalRule("LETTER", "[a-z]"));
            builder.AddRule(new LexerInternalRule("CHARACTER", "[a-z0-9]"));
            builder.AddRule(new LexerInternalRule("DIGIT", "[0-9]"));
            builder.AddRule(new LexerInternalRule("SPECIAL", "[_,\\.\\:\\!%\\#\\-;\\<>\\^]"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.WHITESPACE, "WHITESPACE", "[ \t]*", (SpiceLexerState state) => { return LexerRuleResult.IgnoreToken; }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.ASTERIKS, "ASTERIKS", "\\*"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.PLUS, "PLUS", "\\+"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.MINUS, "MINUS", "-"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.DOT, "DOT", "\\."));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.COMMA, "COMMA", ","));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.DELIMITER, "DELIMITER", @"(\(|\)|\|)"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.EQUAL, "EQUAL", @"="));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.NEWLINE, "NEWLINE", @"(\r\n|\n|\r)",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.ReturnToken;
                }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.CONTINUE, "CONTINUE", @"((\r\n|\n|\r)\+)",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.IgnoreToken;
                }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.ENDS, "ENDS", ".ends"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.END, "END", ".end"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.VALUE, "VALUE", @"(([+-])?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+|[tgmkunpf](<LETTER>)*)?[vH]?)"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.COMMENT, "COMMENT", "[^\r\n]+",
                null,
                (SpiceLexerState state) => {
                    if (state.PreviousTokenType.HasValue && state.PreviousTokenType.Value == (int)SpiceToken.ASTERIKS)
                    {
                        return LexerRuleUseState.Use;
                    }
                    return LexerRuleUseState.Skip;
                }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.TITLE, "TITLE", "[^\r\n]+",
              null,
              (SpiceLexerState state) => {
                  if (state.LineNumber == 1)
                  {
                      return LexerRuleUseState.Use;
                  }
                  return LexerRuleUseState.Skip;
              }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.STRING, "STRING", @"""[^\r\n]+\"""));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.EXPRESSION, "EXPRESSION", "{[^{}]*}"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.REFERENCE, "REFERENCE", "@<WORD>"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.WORD, "WORD", "(<LETTER>(<CHARACTER>|<SPECIAL>)*)"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceToken.IDENTIFIER, "IDENTIFIER", "((<CHARACTER>|_)(<CHARACTER>|<SPECIAL>)*)"));
            this.grammar = builder.GetGrammar();
        }

        public IEnumerable<Token> GetTokens(string text)
        {
            var state = new SpiceLexerState();
            var lexer = new Lexer<SpiceLexerState>(this.grammar, new LexerOptions() { SingleLineTokens = true });
            return lexer.GetTokens(text, state);
        }
    }
}
