using Xunit;
using PrefixLexer = SpiceSharpParser.Lexers.BusPrefix.Lexer;
using PrefixTokenType = SpiceSharpParser.Lexers.BusPrefix.TokenType;
using SuffixLexer = SpiceSharpParser.Lexers.BusSuffix.Lexer;
using SuffixTokenType = SpiceSharpParser.Lexers.BusSuffix.TokenType;

namespace SpiceSharpParser.Tests.Lexers
{
    public class BusLexerTests
    {
        [Fact]
        public void PrefixLexerResetReturnsToFirstToken()
        {
            var lexer = new PrefixLexer("A<1>");
            lexer.ReadToken();

            lexer.Reset();
            lexer.ReadToken();

            Assert.Equal(PrefixTokenType.Letter, lexer.Token);
            Assert.Equal('A', lexer.Current);
        }

        [Fact]
        public void SuffixLexerResetReturnsToFirstToken()
        {
            var lexer = new SuffixLexer("A<1>");
            lexer.ReadToken();

            lexer.Reset();
            lexer.ReadToken();

            Assert.Equal(SuffixTokenType.Letter, lexer.Token);
            Assert.Equal('A', lexer.Current);
        }
    }
}