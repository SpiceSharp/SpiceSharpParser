using SpiceSharpParser.Grammar;
using SpiceSharpParser.Lexer;
using SpiceSharpParser.SpiceLexer;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.SpiceLexer
{
    public class SpiceLexerTest
    {
        [Fact]
        public void OneLetterTitle()
        {
            // lexer can't find matching token for remaining text '}'
            var tokensStr = "T";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.TITLE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void LexerException()
        {
            // lexer can't find matching token for remaining text '}'
            var tokensStr = "V1 in gnd 10}";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            Assert.Throws<LexerException>(() => lexer.GetTokens(tokensStr).ToArray());
        }

        [Fact]
        public void TwoStringTest()
        {
            var tokensStr = "\"ele1\"\"ele2\"";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(3, tokens.Length);

            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.STRING);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.STRING);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void StringQuotedTest()
        {
            var tokensStr = "\"ele1\\\"ele2\\\"\"";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.STRING);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void EndAllCapitalTest()
        {
            var tokensStr = ".END";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void EndMixedTest()
        {
            var tokensStr = ".End";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void EndSmallTest()
        {
            var tokensStr = ".end";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void VectorTest()
        {
            var tokensStr = "v(3,0)";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void TitleTest()
        {
            var tokensStr = "Example of title";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);

            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.TITLE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void SimplesNetlistTest()
        {
            var tokensStr = "a\r\n.end\r\n";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(5, tokens.Length);

            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.TITLE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.NEWLINE);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[3].SpiceTokenType == SpiceTokenType.NEWLINE);
            Assert.True(tokens[4].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void LineContinuationTest()
        {
            var tokensStr = "Example of title\nseq.part1\n+seq.part2\n+seq.part3\n";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void Value1Text()
        {
            var tokensStr = "1picofarad";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value2Text()
        {
            var tokensStr = "1pVolt";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value3Text()
        {
            var tokensStr = "1e-12F";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value4Text()
        {
            var tokensStr = "123.3k";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value5Text()
        {
            var tokensStr = "1e-12";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value6Text()
        {
            var tokensStr = "4.00000E+000";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value7Text()
        {
            var tokensStr = ".568";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void IdentifierTest()
        {
            var tokensStr = "1N914";
            SpiceSharpParser.SpiceLexer.SpiceLexer lexer = new SpiceSharpParser.SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.IDENTIFIER);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }
    }
}
