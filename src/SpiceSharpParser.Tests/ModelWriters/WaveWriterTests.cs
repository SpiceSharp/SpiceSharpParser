using System;
using System.IO;
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
    public class WaveWriterTests
    {
        [Fact]
        public void When_ChannelIsOmitted_Expect_ChannelZero()
        {
            var path = Path.GetTempFileName();

            try
            {
                var parameters = new ParameterCollection
                {
                    new AssignmentParameter() { Name = "wavefile", Value = path },
                };
                var writer = new WaveWriter();

                var statements = writer.Generate(parameters, CreateContext(), out _);

                var statement = Assert.Single(statements.OfType<CSharpNewStatement>());
                Assert.EndsWith(", 0, 1)", statement.NewExpression, StringComparison.Ordinal);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void When_ChannelIsExplicit_Expect_ExplicitChannel()
        {
            var path = Path.GetTempFileName();

            try
            {
                var parameters = new ParameterCollection
                {
                    new AssignmentParameter() { Name = "wavefile", Value = path },
                    new AssignmentParameter() { Name = "chan", Value = "1" },
                };
                var writer = new WaveWriter();

                var statements = writer.Generate(parameters, CreateContext(), out _);

                var statement = Assert.Single(statements.OfType<CSharpNewStatement>());
                Assert.EndsWith(", 1, 1)", statement.NewExpression, StringComparison.Ordinal);
            }
            finally
            {
                File.Delete(path);
            }
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