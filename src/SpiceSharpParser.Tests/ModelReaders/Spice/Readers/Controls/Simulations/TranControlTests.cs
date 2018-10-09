using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class TranControlTests
    {
        [Fact]
        public void ReadWithUIC()
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

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.Result.Returns(resultService);
            readingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            // act
            var tranControl = new TransientControl();
            tranControl.Read(control, readingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<Transient>(simulations.First());
            Assert.True(simulations.First().ParameterSets.Get<TimeConfiguration>().UseIc);
            Assert.Equal(1e-5, simulations.First().ParameterSets.Get<TimeConfiguration>().Step);
            Assert.Equal(1e-3, simulations.First().ParameterSets.Get<TimeConfiguration>().FinalTime);
            Assert.Equal(1e-4, simulations.First().ParameterSets.Get<TimeConfiguration>().MaxStep);
        }

        [Fact]
        public void ReadWithoutUIC()
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

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.Result.Returns(resultService);
            readingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            // act
            var tranControl = new TransientControl();
            tranControl.Read(control, readingContext);

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
