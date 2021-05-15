using SpiceSharpParser.Lexers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.Lexers
{
    public class LexerTests
    {
        [Fact]
        public void EmptyGrammarEmptyText()
        {
            LexerGrammar<LexerState> grammar = new LexerGrammar<LexerState>(new List<LexerTokenRule<LexerState>>(), new List<LexerDynamicRule>());
            Lexer<LexerState> lexer = new Lexer<LexerState>(grammar);
            var tokens = lexer.GetTokens(string.Empty).Tokens;
            Assert.Single(tokens);
        }

        [Fact]
        public void EmptyGrammarNonEmptyText()
        {
            LexerGrammar<LexerState> grammar = new LexerGrammar<LexerState>(new List<LexerTokenRule<LexerState>>(), new List<LexerDynamicRule>());
            Lexer<LexerState> lexer = new Lexer<LexerState>(grammar);
            Assert.False(lexer.GetTokens("Line1\nLine2\n").IsValid);
        }

        [Fact]
        public void NonEmptyGrammarNonEmptyText()
        {
            LexerGrammar<LexerState> grammar = new LexerGrammar<LexerState>(
                new List<LexerTokenRule<LexerState>>()
                {
                    new LexerTokenRule<LexerState>(1, "Text", "[a-zA-Z0-9]*"),
                    new LexerTokenRule<LexerState>(
                        2,
                        "NewLine",
                        "\n",
                        (LexerState state, string lexem) =>
                        {
                            state.LineNumber++;
                            return LexerRuleReturnDecision.ReturnToken;
                        }),
                },
                new List<LexerDynamicRule>());

            Lexer<LexerState> lexer = new Lexer<LexerState>(grammar);
            var s = new LexerState();
            var tokens = lexer.GetTokens("Line1\nLine2\n", s).Tokens.ToArray();
            Assert.Equal(5, tokens.Length);

            Assert.Equal(3, s.LineNumber);
        }
    }
}