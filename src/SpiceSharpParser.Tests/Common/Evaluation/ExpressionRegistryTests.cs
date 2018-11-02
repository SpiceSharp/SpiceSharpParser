using NSubstitute;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Evaluation
{
    public class ExpressionRegistryTests
    {
        [Fact]
        public void AddExpressionWithoutParameters()
        {
            var registry = new ExpressionRegistry(false, false);
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateValueExpression(Arg.Any<string>(), Arg.Any<ExpressionContext>()).Returns(1);

            registry.Add(new NamedExpression("test", "1"), new System.Collections.Generic.List<string>());

            Assert.Empty(registry.GetDependentExpressions("test"));
        }

        [Fact]
        public void AddExpressionWithParameters()
        {
            var registry = new ExpressionRegistry(false, false);
            registry.Add(new Expression("x+1"), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("x"));
        }

        [Fact]
        public void AddExpressionsWithSingleParameter()
        {
            var registry = new ExpressionRegistry(false, false);
            registry.Add(new Expression("x+1"), new System.Collections.Generic.List<string>() { "x" });
            registry.Add(new Expression("y+1"), new System.Collections.Generic.List<string>() { "y" });
            registry.Add(new Expression("x+1"), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("y"));
            Assert.Equal(2, registry.GetDependentExpressions("x").Count());
        }

        [Fact]
        public void AddExpressionsWithMultipleParameter()
        {
            var registry = new ExpressionRegistry(false, false);
            registry.Add(new Expression("x+y+1"), new System.Collections.Generic.List<string>() { "x", "y" });
            registry.Add(new Expression("y+x+1"), new System.Collections.Generic.List<string>() { "y", "x" });
            registry.Add(new Expression("x+1"), new System.Collections.Generic.List<string>() { "x" });

            Assert.Equal(2, registry.GetDependentExpressions("y").Count());
            Assert.Equal(3, registry.GetDependentExpressions("x").Count());
        }
    }
}
