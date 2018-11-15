using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist;
using SpiceSharpParser.Parsers.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.Tests.Parsers
{
    public class ParseTreeGeneratorTests
    {
        [Fact]
        public void NetListWithEmptyComment()
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

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Netlist);
            Assert.NotNull(root);
        }

        [Fact]
        public void SimplestNetlist()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.TITLE, "Example of title"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.EOF, null),
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Netlist);
            Assert.NotNull(root);
        }

        [Fact]
        public void SimplestNetlistWithNewLineAfterEnd()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.TITLE, "Example of title"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.EOF, null),
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Netlist);
            Assert.NotNull(root);
        }

        [Fact]
        public void ComponentStatement()
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

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void CommentStatement()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.COMMENT, "*comment"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void SubcktStatement()
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

            var parser = new ParseTreeGenerator(false, false);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void ModelStatement()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.DOT, "."),
                new SpiceToken(SpiceTokenType.WORD, "model"),
                new SpiceToken(SpiceTokenType.WORD, "npn"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Statement);
        }

        [Fact]
        public void ParameterEqualWithArgument()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.WORD, "12")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void ParameterEqualWithTwoArguments()
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
                new SpiceToken(SpiceTokenType.WORD, "12")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);
            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void ParameterEqualWithFourArguments()
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
                new SpiceToken(SpiceTokenType.WORD, "12")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.Parameter);
            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterEqual, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void NetlistEndingWithoutNewline()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.EOF, ""),
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.NetlistEnding);
            Assert.NotNull(root);
        }

        [Fact]
        public void NetlistEndingWithNewline()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.END, ".end"),
                new SpiceToken(SpiceTokenType.NEWLINE, "\n"),
                new SpiceToken(SpiceTokenType.EOF, ""),
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, Symbols.NetlistEnding);
            Assert.NotNull(root);
        }
   
        [Fact]
        public void ParameterEqualWithoutArguments()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "12")
            };

            var parser = new ParseTreeGenerator(false, true);
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
        public void ParameterBracketBasic()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "D1"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "BF"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
        }

        [Fact]
        public void ParameterBracket()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.WORD, "0"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);

        }

        [Fact]
        public void ParameterBracketEqualSeqence()
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
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };


            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
        }

        [Fact]
        public void ParameterBracketEqualSeqenceAdvanced()
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
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode tree = parser.GetParseTree(tokens, Symbols.Parameter);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
        }

        [Fact]
        public void BraketParameterVector()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "0"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, Symbols.Parameter);
        }

        [Fact]
        public void BraketParameterVectorThreeElements()
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
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, Symbols.Parameter);
        }

        [Fact]
        public void BraketParameterMixed()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "d"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.WORD, "tt"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "0.75ns"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };


            var parser = new ParseTreeGenerator(false, true);
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, Symbols.Parameter);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(Symbols.ParameterBracket, child.Name);
            Assert.Equal(4, child.Children.Count);
            var parameters = (child.Children[2] as ParseTreeNonTerminalNode).Children[0] as ParseTreeNonTerminalNode;
            Assert.Equal(Symbols.Parameters, parameters.Name);
            Assert.Equal(3, parameters.Children.Count);
        }
    }
}
