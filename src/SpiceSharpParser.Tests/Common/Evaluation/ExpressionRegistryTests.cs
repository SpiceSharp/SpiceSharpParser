using System.Linq;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using Xunit;

namespace SpiceSharpParser.Tests.Evaluation
{
    public class ExpressionRegistryTests
    {
        [Fact]
        public void CloneUsesOneClonedNamedExpressionAcrossDependencies()
        {
            var registry = new ExpressionRegistry(false, false);
            var original = new NamedExpression("result", "first + second");
            registry.Add(original, new[] { "first", "second" });

            var clone = registry.Clone();

            var clonedExpression = clone.GetExpression("result");
            Assert.NotSame(original, clonedExpression);
            Assert.Same(clonedExpression, clone.GetDependentExpressions("first").Single());
            Assert.Same(clonedExpression, clone.GetDependentExpressions("second").Single());
        }
    }
}