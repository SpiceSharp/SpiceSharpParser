using SpiceSharpParser.Lexers;
using SpiceSharpParser.Lexers.Netlist.Spice;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.Lexers.Spice
{
    public class SpiceLexerTests
    {
        [Fact]
        public void ReferenceWithFirstNumber()
        {
            var tokensStr = "@1N914[Is]";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.Equal(2, tokens.Length);
        }

        [Fact]
        public void BlockCommentSmartSpiceExtended()
        {
            var tokensStr = "*c1\n#com\n k mxanjsh\n .endl\n.options\n.end\n.end\n~~~/._,m\n+&;;d .\\., .\n $ d .,xznxzc jhad 34 a\r////rr/r/r/\n#endcom\n*c2\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(6, tokens.Length);
        }

        [Fact]
        public void BlockCommentSmartSpiceBasic()
        {
            var tokensStr = "*c1\n#com\n#endcom\n*c2\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(6, tokens.Length);
        }

        [Fact]
        public void BlockCommentSmartSpiceFromStart()
        {
            var tokensStr = "#com\n#endcom\n*c2\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(4, tokens.Length);
        }

        [Fact]
        public void BlockCommentMultipleSmartSpice()
        {
            var tokensStr = "#com\ncontent1\n#endcom\n#com\ncontent2\n#endcom\n*c2\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(5, tokens.Length);
        }

        [Fact]
        public void Reference()
        {
            var tokensStr = "@X1.X1.R1[i]";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.REFERENCE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void PlusAsName()
        {
            var tokensStr = ".SUBCKT noise_macro_0 + -";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens[3].SpiceTokenType == SpiceTokenType.IDENTIFIER);
            Assert.True(tokens[4].SpiceTokenType == SpiceTokenType.IDENTIFIER);

            Assert.Equal(6, tokens.Length);
        }

        [Fact]
        public void IdenfierWithPlus()
        {
            var tokensStr = "CMP2_IN+";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[0].Lexem == "CMP2_IN+");
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void OneLetterTitle()
        {
            // lexer can't find matching token for remaining text '}'
            var tokensStr = "T";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
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
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            Assert.Throws<LexerException>(() => lexer.GetTokens(tokensStr).ToArray());
        }

        [Fact]
        public void TwoStringTest()
        {
            var tokensStr = "\"ele1\"\"ele2\"";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(3, tokens.Length);

            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.DOUBLE_QUOTED_STRING);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.DOUBLE_QUOTED_STRING);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void StringQuotedTest()
        {
            var tokensStr = "\"ele1\\\"ele2\\\"\"";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.DOUBLE_QUOTED_STRING);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void EndAllCapitalTest()
        {
            var tokensStr = ".END";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void EndMixedTest()
        {
            var tokensStr = ".End";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void EndSmallTest()
        {
            var tokensStr = ".end";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.END);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void VectorTest()
        {
            var tokensStr = "v(3,0)";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void TitleTest()
        {
            var tokensStr = "Example of title";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(2, tokens.Length);

            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.TITLE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void SimplesNetlistTest()
        {
            var tokensStr = "a\r\n.end\r\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
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
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void LineNumbersTest()
        {
            var tokensStr = "Example of title\n*comment1\nseq.part1\n+seq.part2\n+seq.part3\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(9, tokens.Length);

            Assert.Equal(1, tokens[0].LineNumber); // title
            Assert.Equal(1, tokens[1].LineNumber); // \n
            Assert.Equal(2, tokens[2].LineNumber); // comment1
            Assert.Equal(2, tokens[3].LineNumber); // \n
            Assert.Equal(3, tokens[4].LineNumber); // seq.part1
            Assert.Equal(4, tokens[5].LineNumber); // seq.part2
            Assert.Equal(5, tokens[6].LineNumber); // seq.part3
            Assert.Equal(5, tokens[7].LineNumber); // \n
            Assert.Equal(5, tokens[8].LineNumber); // EOF
            Assert.Equal(SpiceTokenType.EOF, tokens[8].SpiceTokenType);
        }

        [Fact]
        public void LineContinuationWithSpacesTest()
        {
            var tokensStr = "Example of title\nseq.part1\n     \t    +seq.part2\n  +seq.part3\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = true });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(7, tokens.Length);
        }

        [Fact]
        public void LineContinuationCurrentLineTest()
        {
            var tokensStr = "seq.part1 \\\\\nseq.part2\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.Equal(4, tokens.Length);
        }

        [Fact]
        public void Value1Text()
        {
            var tokensStr = "1picofarad";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value2Text()
        {
            var tokensStr = "1pVolt";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value3Text()
        {
            var tokensStr = "1e-12F";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value4Text()
        {
            var tokensStr = "123.3k";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value5Text()
        {
            var tokensStr = "1e-12";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value6Text()
        {
            var tokensStr = "4.00000E+000";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void Value7Text()
        {
            var tokensStr = ".568";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.VALUE);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void IdentifierTest()
        {
            var tokensStr = "1N914";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.IDENTIFIER);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void PathTest()
        {
            var tokensStr = "Resources\\pwl_comma.txt";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void CommentPSpiceTest()
        {
            var tokensStr = "V12 ; test";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void CommentHSpiceTest()
        {
            var tokensStr = "V12 $ test";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void StringWithDollarTest()
        {
            var tokensStr = "V12 \"$ test\"";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 3);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.DOUBLE_QUOTED_STRING);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void StringWithSemicolonTest()
        {
            var tokensStr = "V12 \"; test\"";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();

            Assert.True(tokens.Length == 3);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.DOUBLE_QUOTED_STRING);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void BracketParameterTest()
        {
            var tokensStr = "1N914(N)";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.Equal(5, tokens.Length);
        }

        [Fact]
        public void Distribution()
        {
            var tokensStr = "DEV/gauss";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void IncludeParent()
        {
            var tokensStr = ".INCLUDE ../lib/diodes.lib";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 4);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.DOT);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[3].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void IncludeParentInQuotes()
        {
            var tokensStr = ".INCLUDE \"../lib/diodes.lib\"";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 4);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.DOT);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.DOUBLE_QUOTED_STRING);
            Assert.True(tokens[3].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void IncludeCurrent()
        {
            var tokensStr = ".INCLUDE ./lib/diodes.lib";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 4);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.DOT);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[2].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[3].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void PercentSimpleTest()
        {
            var tokensStr = "5%";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.PERCENT);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void WordTest()
        {
            var tokensStr = "aaaa \\\\\n";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.WORD);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }

        [Fact]
        public void PercentDotTest()
        {
            var tokensStr = "5.4%";
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerSettings { HasTitle = false });
            var tokens = lexer.GetTokens(tokensStr).ToArray();
            Assert.True(tokens.Length == 2);
            Assert.True(tokens[0].SpiceTokenType == SpiceTokenType.PERCENT);
            Assert.True(tokens[1].SpiceTokenType == SpiceTokenType.EOF);
        }
    }
}