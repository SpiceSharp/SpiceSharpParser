using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using System.Linq;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.Evaluation
{
    public class ExpressionRegistryTest
    {
        [Fact]
        public void AddExpressionWithoutParametersTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new DoubleExpression("1", (double d) => { }), new System.Collections.Generic.List<string>());

            Assert.Empty(registry.GetDependedExpressions("test"));
        }

        [Fact]
        public void AddExpressionWithParametersTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new DoubleExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependedExpressions("x"));
        }

        [Fact]
        public void AddExpressionsWithSingleParameterTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new DoubleExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });
            registry.Add(new DoubleExpression("y+1", (double d) => { }), new System.Collections.Generic.List<string>() { "y" });
            registry.Add(new DoubleExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependedExpressions("y"));
            Assert.Equal(2, registry.GetDependedExpressions("x").Count());
        }

        [Fact]
        public void AddExpressionsWithMultipleParameterTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new DoubleExpression("x+y+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x", "y" });
            registry.Add(new DoubleExpression("y+x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "y", "x" });
            registry.Add(new DoubleExpression("x+1", (double d) => { }), new System.Collections.Generic.List<string>() { "x" });

            Assert.Equal(2, registry.GetDependedExpressions("y").Count());
            Assert.Equal(3, registry.GetDependedExpressions("x").Count());
        }
    }
}
