using NLex;
using SpiceNetlist;
using System.Collections.Generic;

namespace SpiceSharpLex
{
    public class SpiceLexer
    {
        LexerGrammar grammar;

        public SpiceLexer()
        {
            var builder = new LexerGrammarBuilder<SpiceLexerState>();

            builder.AddRule(new LexerInternalRule("LETTER", "[a-z]"));
            builder.AddRule(new LexerInternalRule("CHARACTER", "[a-z0-9]"));
            builder.AddRule(new LexerInternalRule("DIGIT", "[0-9]"));
            builder.AddRule(new LexerInternalRule("SPECIAL", "[_,.:!%#-;/<>\\^]"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.WHITESPACE, "WHITESPACE", "[ \t]*", (SpiceLexerState state) => { return LexerRuleResult.IgnoreToken; }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.ASTERIKS, "ASTERIKS", "\\*"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.PLUS, "PLUS", "\\+"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.MINUS, "MINUS", "-"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.DOT, "DOT", "\\."));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.COMMA, "COMMA", ","));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.DELIMITER, "DELIMITER", @"(=|\(|\)|\|)"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.NEWLINE, "NEWLINE", @"(\r\n|\n|\r)",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.ReturnToken;
                }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.CONTINUE, "CONTINUE", @"((\r\n|\n|\r)\+)",
                (SpiceLexerState state) =>
                {
                    state.LineNumber++;
                    return LexerRuleResult.IgnoreToken;
                }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.ENDS, "ENDS", ".ends"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.END, "END", ".end"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.VALUE, "VALUE", @"(([+-])?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+|[tgmkunpf](<LETTER>)*)?)"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.COMMENT, "COMMENT", "[^\r\n]+",
                null,
                (SpiceLexerState state) => {
                    if (state.PreviousTokenType.HasValue && state.PreviousTokenType.Value == (int)SpiceTokenType.ASTERIKS)
                    {
                        return LexerRuleState.Use;
                    }
                    return LexerRuleState.Skip;
                }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.TITLE, "TITLE", "[^\r\n]+",
              null,
              (SpiceLexerState state) => {
                  if (state.LineNumber == 1)
                  {
                      return LexerRuleState.Use;
                  }
                  return LexerRuleState.Skip;
              }));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.STRING, "STRING", @"""[^\r\n]+\"""));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.EXPRESSION, "EXPRESSION", "{[^{}]*}"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.REFERENCE, "REFERENCE", "@<WORD>"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.WORD, "WORD", "<LETTER>(<CHARACTER>|<SPECIAL>)*"));
            builder.AddRule(new LexerTokenRule<SpiceLexerState>((int)SpiceTokenType.IDENTIFIER, "IDENTIFIER", "(<CHARACTER>|_)(<CHARACTER>|<SPECIAL>)*"));
            this.grammar = builder.GetGrammar();
        }

        public IEnumerable<Token> GetTokens(string text)
        {
            var state = new SpiceLexerState();
            var lexer = new Lexer<SpiceLexerState>(this.grammar);
            lexer.SetState(state);
            return lexer.GetTokens(text);
        }
    }
}
