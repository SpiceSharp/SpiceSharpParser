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

        [Fact]
        public void When_UnaryTildeIsParsedInLtspiceMode_Expect_NotNode()
        {
            var node = Parser.Parse(
                Lexer.FromString("~0", CompatibilityOptions.LTspice),
                true);

            Assert.Equal(NodeTypes.Not, node.NodeType);
        }

        [Fact]
        public void When_UnaryTildeIsParsedInPspiceMode_Expect_NotNode()
        {
            var node = Parser.Parse(
                Lexer.FromString("~0", CompatibilityOptions.PSpice),
                true);

            Assert.Equal(NodeTypes.Not, node.NodeType);
        }

        [Fact]
        public void When_UnaryTildeIsParsedByDefault_Expect_Error()
        {
            Assert.Throws<SpiceSharpParser.Parsers.Expression.ParserException>(
                () => Parser.Parse(Lexer.FromString("~0"), true));
        }

        [Theory]
        [InlineData("xor(0,1)")]
        [InlineData("XOR(1,0)")]
        public void When_XorFunctionIsParsedInLtspiceMode_Expect_XorNode(string expression)
        {
            var node = Parser.Parse(
                Lexer.FromString(expression, CompatibilityOptions.LTspice),
                true);

            Assert.Equal(NodeTypes.Xor, node.NodeType);
        }

        [Fact]
        public void When_XorFunctionIsParsedWithoutLtspiceMode_Expect_FunctionNode()
        {
            var node = Parser.Parse(Lexer.FromString("xor(0,1)"), true);

            Assert.Equal(NodeTypes.Function, node.NodeType);
        }

        [Theory]
        [InlineData("xor(0)")]
        [InlineData("xor(0,1,0)")]
        public void When_XorFunctionHasWrongArityInLtspiceMode_Expect_TargetedError(string expression)
        {
            var exception = Assert.Throws<SpiceSharpParser.Parsers.Expression.ParserException>(
                () => Parser.Parse(Lexer.FromString(expression, CompatibilityOptions.LTspice), true));

            Assert.Contains("xor", exception.Message, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("two arguments", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("rand(0.25)", "rand")]
        [InlineData("random(0.25)", "random")]
        [InlineData("white(0.25)", "white")]
        public void When_LtspiceBehavioralRandomFunctionIsParsed_Expect_LoweredExpression(
            string expression,
            string functionName)
        {
            var node = Parser.Parse(
                Lexer.FromString(expression, CompatibilityOptions.LTspice),
                true);

            Assert.DoesNotContain(
                functionName + "(",
                node.ToString(),
                System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("floor", node.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void When_ZeroArgumentRandomIsParsedInLtspiceMode_Expect_ExistingExtensionIsPreserved()
        {
            var node = Assert.IsType<FunctionNode>(Parser.Parse(
                Lexer.FromString("random()", CompatibilityOptions.LTspice),
                true));

            Assert.Equal("random", node.Name, ignoreCase: true);
            Assert.Empty(node.Arguments);
        }

        [Theory]
        [InlineData("rand()", "rand")]
        [InlineData("rand(1,2)", "rand")]
        [InlineData("random(1,2)", "random")]
        [InlineData("white()", "white")]
        [InlineData("white(1,2)", "white")]
        public void When_LtspiceBehavioralRandomFunctionHasWrongArity_Expect_TargetedError(
            string expression,
            string functionName)
        {
            var exception = Assert.Throws<SpiceSharpParser.Parsers.Expression.ParserException>(
                () => Parser.Parse(
                    Lexer.FromString(expression, CompatibilityOptions.LTspice),
                    true));

            Assert.Contains(functionName, exception.Message, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("one argument", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void When_LtspiceBehavioralRandomFunctionsAreEvaluated_Expect_StepAndSmoothRelationships()
        {
            var builder = new SpiceSharpBehavioral.Builders.Direct.RealBuilder();
            SpiceSharpBehavioral.Builders.Direct.RealBuilderHelper.RegisterDefaultFunctions(builder);
            var expressionParser = new SpiceSharpParser.Common.ExpressionParser(
                builder,
                true,
                CompatibilityOptions.LTspice);

            var rand0 = expressionParser.Evaluate("rand(0)");
            var rand1 = expressionParser.Evaluate("rand(1)");
            var midpoint = (rand0 + rand1) / 2.0;

            Assert.Equal(0.5, rand0, 12);
            Assert.Equal(rand0, expressionParser.Evaluate("rand(0.999)"), 12);
            Assert.Equal(rand1, expressionParser.Evaluate("rand(1.999)"), 12);
            Assert.Equal(midpoint, expressionParser.Evaluate("random(0.5)"), 12);
            Assert.Equal(midpoint - 0.5, expressionParser.Evaluate("white(0.5)"), 12);
            Assert.Equal(0.0, expressionParser.Evaluate("white(0)"), 12);
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
        public void When_PowerOperatorIsParsed_Expect_PowerNode(string expression)
        {
            var node = Parser.Parse(Lexer.FromString(expression), true);

            Assert.Equal(NodeTypes.Pow, node.NodeType);
        }

        [Fact]
        public void When_CaretIsParsedByDefault_Expect_Spice3PowerNode()
        {
            var node = Parser.Parse(Lexer.FromString("2^3"), true);

            Assert.Equal(NodeTypes.Pow, node.NodeType);
        }

        [Fact]
        public void When_CaretIsParsedInPspiceMode_Expect_XorNode()
        {
            var node = Parser.Parse(
                Lexer.FromString("1^0", CompatibilityOptions.PSpice),
                true);

            Assert.Equal(NodeTypes.Xor, node.NodeType);
        }

        [Fact]
        public void When_CaretIsParsedInLtspiceMode_Expect_PowerNode()
        {
            var node = Parser.Parse(
                Lexer.FromString("2^3", CompatibilityOptions.LTspice),
                true);

            Assert.Equal(NodeTypes.Pow, node.NodeType);
        }

        [Fact]
        public void When_PspiceLogicalOperatorsAreParsed_Expect_AndBeforeXorBeforeOr()
        {
            var orNode = Assert.IsType<BinaryOperatorNode>(
                Parser.Parse(
                    Lexer.FromString("1|0^1&0", CompatibilityOptions.PSpice),
                    true));
            var xorNode = Assert.IsType<BinaryOperatorNode>(orNode.Right);
            var andNode = Assert.IsType<BinaryOperatorNode>(xorNode.Right);

            Assert.Equal(NodeTypes.Or, orNode.NodeType);
            Assert.Equal(NodeTypes.Xor, xorNode.NodeType);
            Assert.Equal(NodeTypes.And, andNode.NodeType);
        }
    }
}
