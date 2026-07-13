using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Components;
using SpiceSharpParser.Parsers.Expression;
using Xunit;

namespace SpiceSharpParser.Tests.ModelWriters
{
    public class BehavioralRandomFunctionWriterTests
    {
        [Fact]
        public void When_LtspiceBehavioralRandomFunctionsAreWritten_Expect_RuntimeMathLowering()
        {
            var component = new Component(
                "BNOISE",
                new ParameterCollection(
                    new List<Parameter>
                    {
                        new IdentifierParameter("out"),
                        new IdentifierParameter("0"),
                        new AssignmentParameter
                        {
                            Name = "V",
                            Value = "rand(time)+random(time)+white(time)",
                        },
                    }),
                lineInfo: null);

            var writer = new ArbitraryBehavioralWriter();
            var lines = writer.Write(component, CreateContext());
            var expression = Assert.Single(
                lines.OfType<CSharpAssignmentStatement>(),
                line => line.Left.EndsWith(".Parameters.Expression", StringComparison.Ordinal));

            Assert.DoesNotContain("rand(", expression.ValueExpression, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("random(", expression.ValueExpression, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("white(", expression.ValueExpression, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("floor(", expression.ValueExpression, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sin(", expression.ValueExpression, StringComparison.OrdinalIgnoreCase);
        }

        private static WriterContext CreateContext()
        {
            var parser = new ExpressionParser(
                new SpiceSharpBehavioral.Builders.Direct.RealBuilder(),
                false,
                CompatibilityOptions.LTspice);

            return new WriterContext
            {
                CaseSettings = new SpiceNetlistCaseSensitivitySettings(),
                EvaluationContext = new EvaluationContext(parser, CompatibilityOptions.LTspice),
            };
        }
    }
}
