using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist;
using SpiceSharpParser.Parsers.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist.Spice.Internals;
using Xunit;

namespace SpiceSharpParser.Tests.Parsers
{
    public class ParseTreeGeneratorTests
    {
        [Fact]
        public void When_NetlistHasEmptyComment_Expect_NoException()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.TITLE, "Example of title"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.COMMENT, "*"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.EOF, null),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Netlist);
            Assert.NotNull(root);
        }

        [Fact]
        public void When_NetlistIsVerySimple_Expect_NoException()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.TITLE, "Example of title"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.EOF, null),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Netlist);
            Assert.NotNull(root);
        }

        [Fact]
        public void When_NetlistHasNewLineAfterEnd_Expect_NoException()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.TITLE, "Example of title"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.EOF, null),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Netlist);
            Assert.NotNull(root);
        }

        [Fact]
        public void When_ComponentStatementIsParsed_Expect_NoException()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "L1"),
                new SpiceToken(SpiceTokenType.VALUE, "5"),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.VALUE, "3MH"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
            };

            var parser = new ParseTreeGenerator(true);
            parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void When_CommentStatementIsParsed_Expect_NoException()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.COMMENT, "*comment"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
            };

            var parser = new ParseTreeGenerator(true);
            parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void When_SubcktStatementIsParsed_Expect_NoException()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.DOT, "."),
                new SpiceToken(SpiceTokenType.WORD, "subckt"),
                new SpiceToken(SpiceTokenType.WORD, "amp"),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.ENDS, ".ends"),
                new SpiceToken(SpiceTokenType.WORD, "amp"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
            };

            var parser = new ParseTreeGenerator(false);
            parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void When_ModelStatementIsParsed_Expect_NoException()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.DOT, "."),
                new SpiceToken(SpiceTokenType.WORD, "model"),
                new SpiceToken(SpiceTokenType.WORD, "npn"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
            };

            var parser = new ParseTreeGenerator(true);
            parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void When_ParameterEqualWithArgumentIsParsed_Expect_Reference()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.WORD, "12"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void When_ParameterEqualWithTwoArgumentsIsParsed_Expect_Reference()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.WORD, "1"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.WORD, "12"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);
            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void When_ParameterEqualWithFourArgumentsIsParsed_Expect_Reference()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "fun"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.WORD, "12"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);
            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void When_NetlistHasNoNewlineAfterEnd_Expect_NoException()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.EOF, string.Empty),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.NetlistEnding);
            Assert.NotNull(root);
        }

        [Fact]
        public void When_NetlistHasNewlineAfterEnd_Expect_NoException()
        {
            var tokens = new SpiceToken[]
                             {
                                 new SpiceToken(SpiceTokenType.END, ".end"),
                                 new SpiceToken(SpiceTokenType.NEWLINE, "\n"), new SpiceToken(SpiceTokenType.EOF, string.Empty),
                             };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.NetlistEnding);
            Assert.NotNull(root);
        }

        [Fact]
        public void When_ParameterEqualWithoutArgumentIsParsed_Expect_Reference()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "12"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Single(child.Children);
            var paramater_equal_single = (root.Children[0] as ParseTreeNonTerminalNode).Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(paramater_equal_single);
            Assert.Equal(Symbols.ParameterEqualSingle, paramater_equal_single.Name);
        }

        [Fact]
        public void When_ParameterBracketIsParsed_Expect_Reference()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "D1"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "BF"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
        }

        [Fact]
        public void When_ParameterBracketWithTwoArgumentsIsParsed_Expect_Reference()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.WORD, "0"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
        }

        [Fact]
        public void When_ParameterBracketWithMultipleParameterEqualIsParsed_Expect_Reference()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "npm"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "a"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "2"),
                new SpiceToken(SpiceTokenType.WORD, "b"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
        }

        [Fact]
        public void When_ParameterBracketWithMultipleParameterEqualEdgeCaseIsParsed_Expect_Reference()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "D"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "Is"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "2.52e-9"),
                new SpiceToken(SpiceTokenType.WORD, "Rs"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "0.568"),
                new SpiceToken(SpiceTokenType.WORD, "N"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "1.752"),
                new SpiceToken(SpiceTokenType.WORD, "Cjo"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "4e-12"),
                new SpiceToken(SpiceTokenType.WORD, "M"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "0.4"),
                new SpiceToken(SpiceTokenType.WORD, "tt"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "20e-9"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(tokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
        }

        [Fact]
        public void When_ParameterBracketVectorIsParsed_Expect_NoException()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "0"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            parser.GetParseTree(vectorTokens, Symbols.Parameter);
        }

        [Fact]
        public void When_ParameterBracketWithTreeValuesIsParsed_Expect_NoException()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "0"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            parser.GetParseTree(vectorTokens, Symbols.Parameter);
        }

        [Fact]
        public void When_ParameterBracketWithMixedContentIsParsed_Expect_Reference()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "d"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.WORD, "tt"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "0.75ns"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
            };

            var parser = new ParseTreeGenerator(true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
            var parameters = (child.Children[2] as ParseTreeNonTerminalNode)?.Children[0] as ParseTreeNonTerminalNode;
            Assert.Equal(Symbols.Parameters, parameters?.Name);
            Assert.Equal(3, parameters?.Children.Count);
        }
    }
}