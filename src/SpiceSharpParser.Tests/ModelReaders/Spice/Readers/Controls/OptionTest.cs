using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharp;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class OptionTest
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

            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("12.2").Returns(12.2);
            evaluator.EvaluateDouble("12.3").Returns(12.3);

            var resultService = new ResultService(new SpiceNetlistReaderResult(new Circuit(), "title"));
            var readingContext = new ReadingContext(
                string.Empty,
                Substitute.For<ISimulationsParameters>(),
                new EvaluatorsContainer(evaluator),
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty),
                null,
                null);

            // act
            var optionControl = new OptionsControl();
            optionControl.Read(control, readingContext);

            // assert
            Assert.Equal(12.2 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.TemperaturesInKelvins[0]);
            Assert.Equal(12.3 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.NominalTemperatureInKelvins);
        }

        [Fact]
        public void SeedTest()
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

            var evaluator = Substitute.For<ISpiceEvaluator>();
            var resultService = new ResultService(new SpiceNetlistReaderResult(new Circuit(), "title"));
            var readingContext = new ReadingContext(
                string.Empty,
                Substitute.For<ISimulationsParameters>(),
                new EvaluatorsContainer(evaluator),
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty),
                null,
                null);

            // act
            var optionControl = new OptionsControl();
            optionControl.Read(control, readingContext);

            // assert
            Assert.Equal(1234, resultService.SimulationConfiguration.Seed);
        }
    }
}
