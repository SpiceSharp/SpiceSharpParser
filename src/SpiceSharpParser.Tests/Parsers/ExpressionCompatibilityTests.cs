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

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(0.9, 0.8786938680574733)]
        [InlineData(2.0, 0.9995042495646668)]
        public void When_UplimFunctionEvaluates_Expect_SmoothUpperLimit(double input, double expected)
        {
            var function = MathFunctions.CreateUplim();

            Assert.Equal(expected, function.Logic("uplim", new[] { input, 1.0, 0.2 }, null), 12);
        }

        [Theory]
        [InlineData(0.0, 1.0004957504353332)]
        [InlineData(1.1, 1.1213061319425267)]
        [InlineData(2.0, 2.0)]
        public void When_DnlimFunctionEvaluates_Expect_SmoothLowerLimit(double input, double expected)
        {
            var function = MathFunctions.CreateDnlim();

            Assert.Equal(expected, function.Logic("dnlim", new[] { input, 1.0, 0.2 }, null), 12);
        }

        [Theory]
        [InlineData(0.0, 10.0)]
        [InlineData(1.5, 15.0)]
        [InlineData(4.0, 30.0)]
        public void When_TableFunctionEvaluates_Expect_ClampedOrInterpolatedValue(double input, double expected)
        {
            var function = MathFunctions.CreateTable();

            Assert.Equal(expected, function.Logic("table", new[] { input, 1.0, 10.0, 2.0, 20.0, 3.0, 30.0 }, null));
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
