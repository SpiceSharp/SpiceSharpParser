using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors.Controls.Simulations
{
    public class DCControlTest
    {
        [Fact]
        public void Process()
        {
            // prepare
            var control = new Control()
            {
                Name = "dc",
                Parameters = new ParameterCollection()
                {
                    new IdentifierParameter("vA1"),
                    new ValueParameter("10"),
                    new ValueParameter("20"),
                    new ValueParameter("3")
                }
            };

            var simulations = new List<Simulation>();

            var resultService = Substitute.For<IResultService>();
            resultService.SimulationConfiguration.Returns(new SimulationConfiguration());
            resultService.Simulations.Returns(simulations);
            resultService.When(x => x.AddSimulation(Arg.Any<DC>())).Do(x => { simulations.Add((DC)x[0]); });

            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.Result.Returns(resultService);

            // act
            var dcControl = new DCControl();
            dcControl.Process(control, processingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<DC>(simulations.First());
        }
    }
}
