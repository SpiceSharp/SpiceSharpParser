using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms;
using SpiceSharpParser.Parsers.Expression;
using Xunit;

namespace SpiceSharpParser.Tests.ModelWriters
{
    public class SFFMWriterTests
    {
        [Fact]
        public void SevenArgumentsWriteDistinctCarrierAndSignalPhases()
        {
            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("2"),
                new ValueParameter("3"),
                new ValueParameter("4"),
                new ValueParameter("5"),
                new ValueParameter("6"),
                new ValueParameter("7"),
            };
            var writer = new SFFMWriter();

            var statements = writer.Generate(parameters, CreateContext(), out _);

            Assert.Contains(
                statements.OfType<CSharpAssignmentStatement>(),
                statement => statement.Left.EndsWith(".CarrierPhase") && statement.ValueExpression == "6");
            Assert.Contains(
                statements.OfType<CSharpAssignmentStatement>(),
                statement => statement.Left.EndsWith(".SignalPhase") && statement.ValueExpression == "7");
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