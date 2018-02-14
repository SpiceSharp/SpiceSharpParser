using SpiceLex;
using SpiceNetlist;
using System.Linq;
using Xunit;

namespace SpiceSharpLex.Tests
{
    public class SpiceLexerTest
    {
        [Fact]
        public void VectorTest()
        {
            var tokensStr = "v(3,0)";
            SpiceLexer lexer = new SpiceLexer(new NLexer.SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void TitleTest()
        {
            var tokensStr = "Example of title";
            SpiceLexer lexer = new SpiceLexer(new NLexer.SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);

            Assert.True(tokens[0].TokenType == (int)SpiceToken.TITLE);
            Assert.True(tokens[1].TokenType == -1);
        }
    }
}
