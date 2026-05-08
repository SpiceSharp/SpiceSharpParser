using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;
using Xunit;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.Tests.Parsers
{
    public class ExpressionCompatibilityTests
    {
        [Fact]
        public void When_FabsFunctionEvaluates_Expect_AbsoluteValue()
        {
            var function = MathFunctions.CreateFabs();

            Assert.Equal(3.0, function.Logic("fabs", new[] { -3.0 }, null));
        }

        [Fact]
        public void When_UnaryBangIsParsed_Expect_NotNode()
        {
            var node = Parser.Parse(Lexer.FromString("!0"), true);

            Assert.Equal(NodeTypes.Not, node.NodeType);
        }

        [Theory]
        [InlineData("1&0")]
        [InlineData("1&&0")]
        public void When_AndOperatorIsParsed_Expect_AndNode(string expression)
        {
            var node = Parser.Parse(Lexer.FromString(expression), true);

            Assert.Equal(NodeTypes.And, node.NodeType);
        }

        [Theory]
        [InlineData("1|0")]
        [InlineData("1||0")]
        public void When_OrOperatorIsParsed_Expect_OrNode(string expression)
        {
            var node = Parser.Parse(Lexer.FromString(expression), true);

            Assert.Equal(NodeTypes.Or, node.NodeType);
        }

        [Theory]
        [InlineData("2**3")]
        [InlineData("2^3")]
        public void When_PowerOperatorIsParsed_Expect_PowerNode(string expression)
        {
            var node = Parser.Parse(Lexer.FromString(expression), true);

            Assert.Equal(NodeTypes.Pow, node.NodeType);
        }
    }
}
