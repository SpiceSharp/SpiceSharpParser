using SpiceGrammar;
using System.Linq;
using Xunit;

namespace SpiceLexer.Tests
{
    public class SpiceLexerTest
    {
        [Fact]
        public void VectorTest()
        {
            var tokensStr = "v(3,0)";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void TitleTest()
        {
            var tokensStr = "Example of title";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);

            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.TITLE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void LineContinuationTest()
        {
            var tokensStr = "Example of title\nseq.part1\n+seq.part2\n+seq.part3\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }
    }
}
