using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.Parsers.Expression;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelWriters
{
    public class ResistorWriterTests
    {
        [Fact]
        public void Test01()
        {
            var component =
                new SpiceSharpParser.Models.Netlist.Spice.Objects.Component(
                    "R1",
                    new Models.Netlist.Spice.Objects.ParameterCollection(
                        new System.Collections.Generic.List<Models.Netlist.Spice.Objects.Parameter>()
                        {
                            new WordParameter("in"),
                            new WordParameter("gnd"),
                            new ValueParameter("1000m"),
                        }),
                    lineInfo: null);

            var writer = new SpiceSharpParser.ModelWriters.CSharp.Entities.Components.ResistorWriter();

            var parser = new ExpressionParser(
                new SpiceSharpBehavioral.Builders.Direct.RealBuilder(),
                false);

            var lines = writer.Write(component, new WriterContext()
            {
                CaseSettings = new SpiceNetlistCaseSensitivitySettings(),
                EvaluationContext = new EvaluationContext(parser),
            });

            Assert.True(lines.Any());
        }

        [Fact]
        public void When_ExpressionUsesCircuitQuantity_Expect_OnlyBehavioralResistor()
        {
            var component = new Component(
                "R1",
                new ParameterCollection(
                    new System.Collections.Generic.List<Parameter>()
                    {
                        new WordParameter("in"),
                        new WordParameter("gnd"),
                        new ExpressionParameter("V(in)", null),
                    }),
                lineInfo: null);

            var writer = new SpiceSharpParser.ModelWriters.CSharp.Entities.Components.ResistorWriter();

            var lines = writer.Write(component, CreateContext());

            var newStatement = Assert.Single(lines.OfType<CSharpNewStatement>());
            Assert.Contains("new BehavioralResistor", newStatement.NewExpression);
        }

        [Fact]
        public void When_ResistorUsesModel_Expect_ModelNameWithoutExtraParenthesis()
        {
            var component = new Component(
                "R1",
                new ParameterCollection(
                    new System.Collections.Generic.List<Parameter>()
                    {
                        new WordParameter("in"),
                        new WordParameter("gnd"),
                        new WordParameter("RMOD"),
                    }),
                lineInfo: null);

            var context = CreateContext();
            context.RegisterModelType("RMOD", "R");

            var writer = new SpiceSharpParser.ModelWriters.CSharp.Entities.Components.ResistorWriter();

            var lines = writer.Write(component, context);

            var newStatement = Assert.Single(lines.OfType<CSharpNewStatement>());
            Assert.Equal(@"new Resistor(""R1"", ""in"", ""gnd"", ""RMOD"")", newStatement.NewExpression);
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
