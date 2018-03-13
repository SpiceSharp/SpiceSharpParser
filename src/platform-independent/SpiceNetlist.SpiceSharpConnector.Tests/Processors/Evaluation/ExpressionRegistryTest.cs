using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using System;
using System.Linq;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.Evaluation
{
    public class ExpressionRegistryTest
    {
        [Fact]
        public void AddTest()
        {
            var registry = new ExpressionRegistry();
            registry.Add(new DoubleExpression("test1", (double d) => { }));

            Assert.Single(registry.GetDependedExpressions("test"));
        }
    }
}
