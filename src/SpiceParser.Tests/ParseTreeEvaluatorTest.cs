using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using Xunit;

namespace SpiceParser.Tests
{
    public class ParseTreeEvaluatorTest
    {
        [Fact]
        public void BracketParameterWithVectorTest()
        {
            // Arrange
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
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceNetlist.SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeEvaluator eval = new ParseTreeEvaluator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<BracketParameter>(spiceObject);
            Assert.True(((BracketParameter)spiceObject).Name == "v");
            Assert.True(((BracketParameter)spiceObject).Parameters.Count == 1);
            Assert.True(((BracketParameter)spiceObject).Parameters[0] is VectorParameter);
            Assert.True((((BracketParameter)spiceObject).Parameters[0] as VectorParameter).Elements.Count == 2);
        }

        [Fact]
        public void BracketParameterWithSingleParametersTest()
        {
            // Arrange
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "pulse"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.VALUE, "1"),
                new NLexer.Token((int)SpiceToken.VALUE, "2"),
                new NLexer.Token((int)SpiceToken.VALUE, "3"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceNetlist.SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeEvaluator eval = new ParseTreeEvaluator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<BracketParameter>(spiceObject);
            Assert.True(((BracketParameter)spiceObject).Name == "pulse");
            Assert.True(((BracketParameter)spiceObject).Parameters[0] is SingleParameter);
            Assert.True(((BracketParameter)spiceObject).Parameters[1] is SingleParameter);
            Assert.True(((BracketParameter)spiceObject).Parameters[2] is SingleParameter);
        }

        [Fact]
        public void AssigmentParameterWithVectorTest()
        {
            // Arrange
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.COMMA, ","),
                new NLexer.Token((int)SpiceToken.WORD, "0"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "13"),
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceNetlist.SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeEvaluator eval = new ParseTreeEvaluator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<AssignmentParameter>(spiceObject);
            Assert.True(((AssignmentParameter)spiceObject).Name == "v");
            Assert.True(((AssignmentParameter)spiceObject).Arguments.Count == 2);
            Assert.True(((AssignmentParameter)spiceObject).Value == "13");
        }

        [Fact]
        public void BracketParameterWithAssigmentsTest()
        {
            // Arrange
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "n1"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "2"),
                new NLexer.Token((int)SpiceToken.WORD, "n3"),
                new NLexer.Token((int)SpiceToken.EQUAL, "="),
                new NLexer.Token((int)SpiceToken.VALUE, "3"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
            };

            var parser = new SpiceParser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceNetlist.SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeEvaluator eval = new ParseTreeEvaluator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<BracketParameter>(spiceObject);
            Assert.True(((BracketParameter)spiceObject).Name == "v");
            Assert.True(((BracketParameter)spiceObject).Parameters.Count == 2);
            Assert.True(((BracketParameter)spiceObject).Parameters[0] is AssignmentParameter);
            Assert.True(((BracketParameter)spiceObject).Parameters[1] is AssignmentParameter);
        }
    }
}
