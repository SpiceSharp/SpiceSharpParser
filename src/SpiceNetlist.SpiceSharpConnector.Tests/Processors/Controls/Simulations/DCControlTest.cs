using NSubstitute;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.Controls.Simulations
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
