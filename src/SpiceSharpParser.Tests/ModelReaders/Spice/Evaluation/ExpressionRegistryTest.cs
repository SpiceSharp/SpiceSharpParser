using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
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
            registry.Add(new NamedExpression("test", "1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; }, null), new System.Collections.Generic.List<string>());

            Assert.Empty(registry.GetDependentExpressions("test"));
        }

        [Fact]
        public void AddExpressionWithParametersTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new EvaluatorExpression("x+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; }, null), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("x"));
        }

        [Fact]
        public void AddExpressionsWithSingleParameterTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new EvaluatorExpression("x+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; },null), new System.Collections.Generic.List<string>() { "x" });
            registry.Add(new EvaluatorExpression("y+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; },null), new System.Collections.Generic.List<string>() { "y" });
            registry.Add(new EvaluatorExpression("x+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; },null), new System.Collections.Generic.List<string>() { "x" });

            Assert.Single(registry.GetDependentExpressions("y"));
            Assert.Equal(2, registry.GetDependentExpressions("x").Count());
        }

        [Fact]
        public void AddExpressionsWithMultipleParameterTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new EvaluatorExpression("x+y+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; }, null), new System.Collections.Generic.List<string>() { "x", "y" });
            registry.Add(new EvaluatorExpression("y+x+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; }, null), new System.Collections.Generic.List<string>() { "y", "x" });
            registry.Add(new EvaluatorExpression("x+1", (string s, object o, EvaluatorExpression e, IEvaluator evaluator) => { return 0; }, null), new System.Collections.Generic.List<string>() { "x" });

            Assert.Equal(2, registry.GetDependentExpressions("y").Count());
            Assert.Equal(3, registry.GetDependentExpressions("x").Count());
        }
    }
}
