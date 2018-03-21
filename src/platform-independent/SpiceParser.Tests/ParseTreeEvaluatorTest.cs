using SpiceGrammar;
using SpiceLexer;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceParser.Parsing;
using SpiceParser.Translation;
using Xunit;

namespace SpiceParser.Tests
{
    public class ParseTreeParseTreeEvaluatorTest
    {
        [Fact]
        public void VectorTest()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "2"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.VECTOR);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
            var spiceObject = eval.Evaluate(tree) as VectorParameter;

            Assert.Equal(3, spiceObject.Elements.Count);
        }

        [Fact]
        public void VectorLongerTest()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "2"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.VALUE, "4"),
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.VECTOR);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
            var spiceObject = eval.Evaluate(tree) as VectorParameter;

            Assert.Equal(4, spiceObject.Elements.Count);
        }

        [Fact]
        public void BracketParameterWithVectorTest()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.WORD, "0"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
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
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "pulse"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.VALUE, "1"),
                new SpiceToken(SpiceTokenType.VALUE, "2"),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
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
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "out"),
                new SpiceToken(SpiceTokenType.COMMA, ","),
                new SpiceToken(SpiceTokenType.WORD, "0"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "13"),
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
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
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "v"),
                new SpiceToken(SpiceTokenType.DELIMITER, "("),
                new SpiceToken(SpiceTokenType.WORD, "n1"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "2"),
                new SpiceToken(SpiceTokenType.WORD, "n3"),
                new SpiceToken(SpiceTokenType.EQUAL, "="),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.DELIMITER, ")")
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<BracketParameter>(spiceObject);
            Assert.True(((BracketParameter)spiceObject).Name == "v");
            Assert.True(((BracketParameter)spiceObject).Parameters.Count == 2);
            Assert.True(((BracketParameter)spiceObject).Parameters[0] is AssignmentParameter);
            Assert.True(((BracketParameter)spiceObject).Parameters[1] is AssignmentParameter);
        }

        [Fact]
        public void ComponentTest()
        {
            // Arrange
            var vectorTokens = new SpiceToken[]
            {
                new SpiceToken(SpiceTokenType.WORD, "L1"),
                new SpiceToken(SpiceTokenType.VALUE, "5"),
                new SpiceToken(SpiceTokenType.VALUE, "3"),
                new SpiceToken(SpiceTokenType.VALUE, "3MH"),
            };

            var parser = new Parser();
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceGrammarSymbol.COMPONENT);

            // Act
            ParseTreeTranslator eval = new ParseTreeTranslator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<Component>(spiceObject);
            Assert.True(((Component)spiceObject).Name == "L1");
            Assert.True(((Component)spiceObject).PinsAndParameters.Count == 3);
            Assert.True(((Component)spiceObject).PinsAndParameters[0] is ValueParameter);
            Assert.True(((Component)spiceObject).PinsAndParameters[1] is ValueParameter);
            Assert.True(((Component)spiceObject).PinsAndParameters[2] is ValueParameter);
        }
    }
}
