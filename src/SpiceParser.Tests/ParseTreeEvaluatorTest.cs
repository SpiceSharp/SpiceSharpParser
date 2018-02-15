using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using Xunit;

namespace SpiceParser.Tests
{
    public class ParseTreeEvaluatorTest
    {
        [Fact]
        public void VectorParseTreeEvaluate()
        {
            // Arrange
            var parser = new SpiceParser();
            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.COMMA, ","),
                new NLexer.Token((int)SpiceToken.WORD, "0"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
            };
            ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceNetlist.SpiceGrammarSymbol.PARAMETER);

            // Act
            ParseTreeEvaluator eval = new ParseTreeEvaluator();
            var spiceObject = eval.Evaluate(tree);

            // Assert
            Assert.IsType<BracketParameter>(spiceObject);
            Assert.True(((BracketParameter)spiceObject).Name == "v");
            Assert.True(((BracketParameter)spiceObject).Content.Parameters.Count == 1);
            Assert.True(((BracketParameter)spiceObject).Content.Parameters[0] is VectorParameter);
            Assert.True((((BracketParameter)spiceObject).Content.Parameters[0] as VectorParameter).Elements.Count == 2);
        }
    }
}
