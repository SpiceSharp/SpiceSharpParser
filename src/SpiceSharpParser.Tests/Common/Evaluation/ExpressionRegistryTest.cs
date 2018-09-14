using NSubstitute;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Evaluation
{
    public class ExpressionRegistryTest
    {
        [Fact]
        public void AddExpressionWithoutParametersTest()
        {
            var registry = new ExpressionRegistry();
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble(Arg.Any<string>()).Returns(1);

            registry.Add(new NamedEvaluatorExpression("test", "1", evaluator), new System.Collections.Generic.List<string>());

            Assert.Empty(registry.GetDependentExpressions("test"));
        }

        [Fact]
        public void AddExpressionWithParametersTest()
        {
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble(Arg.Any<string>()).Returns(1);
            var registry = new ExpressionRegistry();
            registry.Add(new EvaluatorExpression("x+1", evaluator), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("x"));
        }

        [Fact]
        public void AddExpressionsWithSingleParameterTest()
        {
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble(Arg.Any<string>()).Returns(1);

            var registry = new ExpressionRegistry();
            registry.Add(new EvaluatorExpression("x+1", evaluator), new System.Collections.Generic.List<string>() { "x" });
            registry.Add(new EvaluatorExpression("y+1", evaluator), new System.Collections.Generic.List<string>() { "y" });
            registry.Add(new EvaluatorExpression("x+1", evaluator), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("y"));
            Assert.Equal(2, registry.GetDependentExpressions("x").Count());
        }

        [Fact]
        public void AddExpressionsWithMultipleParameterTest()
        {
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble(Arg.Any<string>()).Returns(1);
            var registry = new ExpressionRegistry();
            registry.Add(new EvaluatorExpression("x+y+1", evaluator), new System.Collections.Generic.List<string>() { "x", "y" });
            registry.Add(new EvaluatorExpression("y+x+1", evaluator), new System.Collections.Generic.List<string>() { "y", "x" });
            registry.Add(new EvaluatorExpression("x+1", evaluator), new System.Collections.Generic.List<string>() { "x" });

            Assert.Equal(2, registry.GetDependentExpressions("y").Count());
            Assert.Equal(3, registry.GetDependentExpressions("x").Count());
        }
    }
}
