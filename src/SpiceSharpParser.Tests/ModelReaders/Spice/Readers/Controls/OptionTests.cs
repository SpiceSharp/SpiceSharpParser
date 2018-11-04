using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharp;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Parsers.Expression;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class OptionTests
    {
        [Fact]
        public void Read()
        {
            // prepare
            var control = new Control()
            {
                Name = "options",
                Parameters = new ParameterCollection()
                {
                    new AssignmentParameter()
                    {
                        Name = "temp",
                        Value = "12.2"
                    },
                    new AssignmentParameter()
                    {
                        Name = "tnom",
                        Value = "12.3"
                    },
                }
            };

            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateValueExpression("12.2", Arg.Any<ExpressionContext>()).Returns(12.2);
            evaluator.EvaluateValueExpression("12.3", Arg.Any<ExpressionContext>()).Returns(12.3);

            var resultService = new ResultService(new SpiceNetlistReaderResult(new Circuit(), "title"));
            var readingContext = new ReadingContext(
              string.Empty,
              new SpiceExpressionParser(),
              new SimulationPreparations(null, null),
              new SimulationEvaluators(evaluator),
              new SimulationExpressionContexts(null),
              resultService,
              new MainCircuitNodeNameGenerator(new string[] { }, true),
              new ObjectNameGenerator(string.Empty),
              new ObjectNameGenerator(string.Empty),
              null,
              null,
              new SpiceEvaluator(),
              new ExpressionContext(),
              new SpiceNetlistCaseSensitivitySettings());

            // act
            var optionControl = new OptionsControl();
            optionControl.Read(control, readingContext);

            // assert
            Assert.Equal(12.2 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.TemperaturesInKelvins[0]);
            Assert.Equal(12.3 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.NominalTemperatureInKelvins);
        }

        [Fact]
        public void Seed()
        {
            // arrange
            var control = new Control()
            {
                Name = "options",
                Parameters = new ParameterCollection()
                {
                    new AssignmentParameter()
                    {
                        Name = "seed",
                        Value = "1234"
                    }
                }
            };

            var evaluator = Substitute.For<IEvaluator>();
            var resultService = new ResultService(new SpiceNetlistReaderResult(new Circuit(), "title"));
            var readingContext = new ReadingContext(
              string.Empty,
              new SpiceExpressionParser(),
              new SimulationPreparations(null, null),
              new SimulationEvaluators(evaluator),
              new SimulationExpressionContexts(null),
              resultService,
              new MainCircuitNodeNameGenerator(new string[] { }, true),
              new ObjectNameGenerator(string.Empty),
              new ObjectNameGenerator(string.Empty),
              null,
              null,
              null,
              null,
              new SpiceNetlistCaseSensitivitySettings());

            // act
            var optionControl = new OptionsControl();
            optionControl.Read(control, readingContext);

            // assert
            Assert.Equal(1234, resultService.SimulationConfiguration.Seed);
        }
    }
}
