using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class DCControlTest
    {
        [Fact]
        public void Read()
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
            

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.Result.Returns(resultService);
            readingContext.CaseSensitivity.Returns(new SpiceSharpParser.Common.CaseSensitivitySettings());
            // act
            var dcControl = new DCControl();
            dcControl.Read(control, readingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<DC>(simulations.First());
        }
    }
}
