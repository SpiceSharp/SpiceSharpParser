using SpiceGrammar;
using System;
using Xunit;

namespace SpiceParser.Tests
{
    public class SpiceParserTest
    {
        [Fact]
        public void ParameterEqualWithArgumentTest()
        {
            var tokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.WORD, "12")
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
            var tokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.COMMA, ","),
                new NLexer.Token((int)SpiceToken.WORD, "1"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.WORD, "12")
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
            var tokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "12")
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
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "pulse"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.VALUE, "4"),
                new NLexer.Token((int)SpiceToken.VALUE, "0"),
                new NLexer.Token((int)SpiceToken.VALUE, "1ns"),
                new NLexer.Token((int)SpiceToken.VALUE, "20ns"),
                new NLexer.Token((int)SpiceToken.VALUE, "40ns"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);
        }

        [Fact]
        public void ParameterBracketTest()
        {
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.COMMA, ","),
                new NLexer.Token((int)SpiceToken.WORD, "0"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
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
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "a"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "2"),
                new NLexer.Token((int)SpiceToken.WORD, "b"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "3"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
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
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.COMMA, ","),
                new NLexer.Token((int)SpiceToken.VALUE, "0"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode root = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);
        }

        [Fact]
        public void BraketParameterMixedTest()
        {
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "d"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.VALUE, "1"),
                new NLexer.Token((int)SpiceToken.WORD, "tt"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "0.75ns"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
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
