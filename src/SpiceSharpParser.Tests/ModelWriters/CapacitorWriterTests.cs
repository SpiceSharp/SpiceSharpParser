using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.Parsers.Expression;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelWriters
{
    public class CapacitorWriterTests
    {
        [Fact]
        public void When_ExpressionUsesCircuitQuantity_Expect_OnlyBehavioralCapacitor()
        {
            var component = new Component(
                "C1",
                new ParameterCollection(
                    new System.Collections.Generic.List<Parameter>()
                    {
                        new WordParameter("in"),
                        new WordParameter("gnd"),
                        new ExpressionParameter("V(in)", null),
                    }),
                lineInfo: null);

            var writer = new SpiceSharpParser.ModelWriters.CSharp.Entities.Components.CapacitorWriter();

            var lines = writer.Write(component, CreateContext());

            var newStatement = Assert.Single(lines.OfType<CSharpNewStatement>());
            Assert.Contains("new BehavioralCapacitor", newStatement.NewExpression);
        }

        private static WriterContext CreateContext()
        {
            var parser = new ExpressionParser(
                new SpiceSharpBehavioral.Builders.Direct.RealBuilder(),
                false);

            return new WriterContext()
            {
                CaseSettings = new SpiceNetlistCaseSensitivitySettings(),
                EvaluationContext = new EvaluationContext(parser),
            };
        }
    }
}
