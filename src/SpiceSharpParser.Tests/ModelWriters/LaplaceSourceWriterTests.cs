using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Components;
using Xunit;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.Tests.ModelWriters
{
    public class LaplaceSourceWriterTests
    {
        [Fact]
        public void When_ELaplaceIsWritten_Expect_LaplaceVoltageControlledVoltageSource()
        {
            var component = new Component(
                "ELOW",
                CreateLaplaceParameters(
                    "V(in)",
                    "1/(1+s*tau)",
                    Assignment("M", "2"),
                    Assignment("TD", "1n")),
                lineInfo: null);

            var writer = new VoltageControlledVoltageSourceWriter();
            var lines = writer.Write(component, CreateContext(("tau", "1u")));

            Assert.Contains(lines.OfType<CSharpNewStatement>(), line => line.NewExpression == @"new LaplaceVoltageControlledVoltageSource(""ELOW"")");
            Assert.Contains(lines.OfType<CSharpCallStatement>(), line => line.CallExpression == @"Connect(""out"", ""0"", ""in"", ""0"")");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Numerator") && line.ValueExpression == "new[] { 2d }");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Denominator") && line.ValueExpression == "new[] { 1d, 1E-06d }");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Delay") && line.ValueExpression == "1E-09d");
        }

        [Fact]
        public void When_GLaplaceEqualsAfterKeywordIsWritten_Expect_LaplaceVoltageControlledCurrentSource()
        {
            var component = new Component(
                "GLOW",
                CreateEqualsAfterKeywordLaplaceParameters(
                    "V(inp,inn)",
                    "gm/(1+s)"),
                lineInfo: null);

            var writer = new VoltageControlledCurrentSourceWriter();
            var lines = writer.Write(component, CreateContext(("gm", "1m")));

            Assert.Contains(lines.OfType<CSharpNewStatement>(), line => line.NewExpression == @"new LaplaceVoltageControlledCurrentSource(""GLOW"")");
            Assert.Contains(lines.OfType<CSharpCallStatement>(), line => line.CallExpression == @"Connect(""out"", ""0"", ""inp"", ""inn"")");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Numerator") && line.ValueExpression == "new[] { 0.001d }");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Denominator") && line.ValueExpression == "new[] { 1d, 1d }");
        }

        [Fact]
        public void When_HLaplaceIsWritten_Expect_LaplaceCurrentControlledVoltageSource()
        {
            var component = new Component(
                "HLOW",
                CreateLaplaceParameters("I(VSENSE)", "1000/(s+1000)"),
                lineInfo: null);

            var writer = new CurrentControlledVoltageSourceWriter();
            var lines = writer.Write(component, CreateContext());

            Assert.Contains(lines.OfType<CSharpNewStatement>(), line => line.NewExpression == @"new LaplaceCurrentControlledVoltageSource(""HLOW"")");
            Assert.Contains(lines.OfType<CSharpCallStatement>(), line => line.CallExpression == @"Connect(""out"", ""0"")");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".ControllingSource") && line.ValueExpression == @"""VSENSE""");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Numerator") && line.ValueExpression == "new[] { 1000d }");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Denominator") && line.ValueExpression == "new[] { 1000d, 1d }");
        }

        [Fact]
        public void When_FLaplaceNoEqualsIsWritten_Expect_LaplaceCurrentControlledCurrentSource()
        {
            var component = new Component(
                "FLOW",
                CreateNoEqualsLaplaceParameters("I(VSENSE)", "gain/(1+s)"),
                lineInfo: null);

            var writer = new CurrentControlledCurrentSourceWriter();
            var lines = writer.Write(component, CreateContext(("gain", "2")));

            Assert.Contains(lines.OfType<CSharpNewStatement>(), line => line.NewExpression == @"new LaplaceCurrentControlledCurrentSource(""FLOW"")");
            Assert.Contains(lines.OfType<CSharpCallStatement>(), line => line.CallExpression == @"Connect(""out"", ""0"")");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".ControllingSource") && line.ValueExpression == @"""VSENSE""");
            Assert.Contains(lines.OfType<CSharpAssignmentStatement>(), line => line.Left.EndsWith(".Parameters.Numerator") && line.ValueExpression == "new[] { 2d }");
        }

        private static WriterContext CreateContext(params (string Name, string Expression)[] parameters)
        {
            var parser = new ExpressionParser(
                new SpiceSharpBehavioral.Builders.Direct.RealBuilder(),
                false);

            var context = new WriterContext()
            {
                CaseSettings = new SpiceNetlistCaseSensitivitySettings(),
                EvaluationContext = new EvaluationContext(parser),
            };

            foreach (var parameter in parameters)
            {
                context.EvaluationContext.ParameterExpressions[parameter.Name] = parameter.Expression;
            }

            return context;
        }

        private static ParameterCollection CreateLaplaceParameters(
            string inputExpression,
            string transferExpression,
            params Parameter[] extraParameters)
        {
            var parameters = new ParameterCollection(
                new List<Parameter>
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    new WordParameter("LAPLACE"),
                    new ExpressionAssignmentParameter(inputExpression, transferExpression, null),
                });

            foreach (var parameter in extraParameters)
            {
                parameters.Add(parameter);
            }

            return parameters;
        }

        private static ParameterCollection CreateNoEqualsLaplaceParameters(
            string inputExpression,
            string transferExpression)
        {
            return new ParameterCollection(
                new List<Parameter>
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    new WordParameter("LAPLACE"),
                    new ExpressionParameter(inputExpression, null),
                    new ExpressionParameter(transferExpression, null),
                });
        }

        private static ParameterCollection CreateEqualsAfterKeywordLaplaceParameters(
            string inputExpression,
            string transferExpression)
        {
            return new ParameterCollection(
                new List<Parameter>
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("LAPLACE", inputExpression),
                    new ExpressionParameter(transferExpression, null),
                });
        }

        private static AssignmentParameter Assignment(string name, string value)
        {
            return new AssignmentParameter
            {
                Name = name,
                Value = value,
            };
        }
    }
}
