using NSubstitute;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Controls.Simulations;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using Xunit;
using SpiceSharpParser.Connector.Processors.Controls;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharp;

namespace SpiceSharpParser.Tests.Connector.Processors.Controls.Simulations
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

            var resultService = new ResultService(new SpiceSharpParser.Connector.SpiceSharpModel(new SpiceSharp.Circuit(), "title"));
            var processingContext = new ProcessingContext(string.Empty,
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
