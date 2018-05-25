using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls.Simulations;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;
using Xunit;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls;
using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharp;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors.Controls.Simulations
{
    public class OptionTest
    {
        [Fact]
        public void Process()
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
            evaluator.EvaluateDouble("12.2").Returns(12.2);
            evaluator.EvaluateDouble("12.3").Returns(12.3);

            var resultService = new ResultService(new SpiceSharpParser.ModelReader.Spice.SpiceModelReaderResult(new SpiceSharp.Circuit(), "title"));
            var processingContext = new ProcessingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var optionControl = new OptionControl();
            optionControl.Process(control, processingContext);

            // assert
            Assert.Equal(12.2 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.TemperaturesInKelvins[0]);
            Assert.Equal(12.3 + Circuit.CelsiusKelvin, resultService.SimulationConfiguration.NominalTemperatureInKelvins);
        }
    }
}
