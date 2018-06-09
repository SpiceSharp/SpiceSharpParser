using SpiceSharpParser.Common;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Evaluation
{
    public class ExpressionRegistryTest
    {
        [Fact]
        public void AddExpressionWithoutParametersTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new ActionExpression("1", (double d) => { }), new System.Collections.Generic.List<string>());

            Assert.Empty(registry.GetDependentExpressions("test"));
        }

        [Fact]
        public void AddExpressionWithParametersTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new ActionExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("x"));
        }

        [Fact]
        public void AddExpressionsWithSingleParameterTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new ActionExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });
            registry.Add(new ActionExpression("y+1", (double d) => { }), new System.Collections.Generic.List<string>() { "y" });
            registry.Add(new ActionExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("y"));
            Assert.Equal(2, registry.GetDependentExpressions("x").Count());
        }

        [Fact]
        public void AddExpressionsWithMultipleParameterTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new ActionExpression("x+y+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x", "y" });
            registry.Add(new ActionExpression("y+x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "y", "x" });
            registry.Add(new ActionExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });

            Assert.Equal(2, registry.GetDependentExpressions("y").Count());
            Assert.Equal(3, registry.GetDependentExpressions("x").Count());
        }
    }
}
