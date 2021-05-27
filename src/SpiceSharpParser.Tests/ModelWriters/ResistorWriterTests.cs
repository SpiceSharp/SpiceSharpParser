using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Components;
using System.Linq;
using SpiceSharpParser.Parsers.Expression;
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

            var writer = new ResistorWriter();

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
    }
}
