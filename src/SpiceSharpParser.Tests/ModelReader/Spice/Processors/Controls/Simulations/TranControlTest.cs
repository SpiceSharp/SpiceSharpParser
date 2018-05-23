using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls.Simulations;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors.Controls.Simulations
{
    public class TranControlTest
    {
        [Fact]
        public void ProcessWithUIC()
        {
            // prepare
            var control = new Control()
            {
                Name = "tran",
                Parameters = new ParameterCollection()
                {
                    new ValueParameter("1e-5"),
                    new ValueParameter("1e-3"),
                    new ValueParameter("1e-4"),
                    new WordParameter("UIC")
                }
            };

            var simulations = new List<Simulation>();

            var resultService = Substitute.For<IResultService>();
            resultService.SimulationConfiguration.Returns(new SimulationConfiguration());
            resultService.Simulations.Returns(simulations);
            resultService.When(x => x.AddSimulation(Arg.Any<Transient>())).Do(x => { simulations.Add((Transient)x[0]); });

            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.Result.Returns(resultService);
            processingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));

            // act
            var tranControl = new TransientControl();
            tranControl.Process(control, processingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<Transient>(simulations.First());
            Assert.True(simulations.First().ParameterSets.Get<TimeConfiguration>().UseIc);
            Assert.Equal(1e-5, simulations.First().ParameterSets.Get<TimeConfiguration>().Step);
            Assert.Equal(1e-3, simulations.First().ParameterSets.Get<TimeConfiguration>().FinalTime);
            Assert.Equal(1e-4, simulations.First().ParameterSets.Get<TimeConfiguration>().MaxStep);
        }

        [Fact]
        public void ProcessWithoutUIC()
        {
            // prepare
            var control = new Control()
            {
                Name = "tran",
                Parameters = new ParameterCollection()
                {
                    new ValueParameter("1e-5"),
                    new ValueParameter("1e-3"),
                    new ValueParameter("1e-4")
                }
            };

            var simulations = new List<Simulation>();

            var resultService = Substitute.For<IResultService>();
            resultService.SimulationConfiguration.Returns(new SimulationConfiguration());
            resultService.Simulations.Returns(simulations);
            resultService.When(x => x.AddSimulation(Arg.Any<Transient>())).Do(x => { simulations.Add((Transient)x[0]); });

            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.Result.Returns(resultService);
            processingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));

            // act
            var tranControl = new TransientControl();
            tranControl.Process(control, processingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<Transient>(simulations.First());
            Assert.False(simulations.First().ParameterSets.Get<TimeConfiguration>().UseIc);
            Assert.Equal(1e-5, simulations.First().ParameterSets.Get<TimeConfiguration>().Step);
            Assert.Equal(1e-3, simulations.First().ParameterSets.Get<TimeConfiguration>().FinalTime);
            Assert.Equal(1e-4, simulations.First().ParameterSets.Get<TimeConfiguration>().MaxStep);
        }
    }
}
