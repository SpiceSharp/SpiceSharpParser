using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using Xunit;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation;
using SpiceSharp;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Readers.Controls.Simulations
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

            var resultService = new ResultService(new SpiceSharpParser.ModelReader.Netlist.Spice.SpiceNetlistReaderResult(new SpiceSharp.Circuit(), "title"));
            var readingContext = new ReadingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var optionControl = new OptionControl();
            optionControl.Read(control, readingContext);

            // assert
            Assert.Equal(12.2 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.TemperaturesInKelvins[0]);
            Assert.Equal(12.3 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.NominalTemperatureInKelvins);
        }
    }
}
