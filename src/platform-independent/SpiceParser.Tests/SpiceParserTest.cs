using SpiceGrammar;
using SpiceLexer;
using Xunit;

namespace SpiceParser.Tests
{
    public class SpiceParserTest
    {
        [Fact]
        public void ParameterEqualWithArgumentTest()
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

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, SpiceGrammarSymbol.PARAMETER);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_EQUAL, child.Name);
            Assert.Equal(6, child.Children.Count);
        }

        [Fact]
        public void ParameterEqualWithArgumentsTest()
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


            var parser = new SpiceParser();
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, SpiceGrammarSymbol.PARAMETER);
            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_EQUAL, child.Name);
            Assert.Equal(8, child.Children.Count);
        }

        [Fact]
        public void ParameterEqualWithoutArgumentsTest()
        {
            var tokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "12")
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode root = parser.GetParseTree(tokens, SpiceGrammarSymbol.PARAMETER);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_EQUAL, child.Name);
            Assert.Single(child.Children);
            var paramater_equal_single = (root.Children[0] as ParseTreeNonTerminalNode).Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(paramater_equal_single);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, paramater_equal_single.Name);
        }


        [Fact]
        public void ParameterBracketSingleParametersTest()
        {
            // pulse(4 0 1ns 1ns 1ns 20ns 40ns)
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "pulse"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.VALUE, "4"),
                new SpiceToken(SpiceTokenType.VALUE, "0"),
                new SpiceToken(SpiceTokenType.VALUE, "1ns"),
                new SpiceToken(SpiceTokenType.VALUE, "20ns"),
                new SpiceToken(SpiceTokenType.VALUE, "40ns"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);
        }

        [Fact]
        public void ParameterBracketTest()
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

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_BRACKET, child.Name);
            Assert.Equal(4, child.Children.Count);

        }

        [Fact]
        public void ParameterBracketEqualSeqenceTest()
        {
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "a"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "2"),
                new SpiceToken(SpiceTokenType.WORD, "b"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };


            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            var child = tree.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_BRACKET, child.Name);
            Assert.Equal(4, child.Children.Count);
        }

        [Fact]
        public void BraketParameterVectorTest()
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

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);
        }

        [Fact]
        public void BraketParameterMixedTest()
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


            var parser = new SpiceParser();
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            var child = root.Children[0] as ParseTreeNonTerminalNode;
            Assert.NotNull(child);
            Assert.Equal(SpiceGrammarSymbol.PARAMETER_BRACKET, child.Name);
            Assert.Equal(4, child.Children.Count);
            var parameters = (child.Children[2] as ParseTreeNonTerminalNode).Children[0] as ParseTreeNonTerminalNode;
            Assert.Equal(SpiceGrammarSymbol.PARAMETERS, parameters.Name);
            Assert.Equal(2, parameters.Children.Count);
        }
    }
}
