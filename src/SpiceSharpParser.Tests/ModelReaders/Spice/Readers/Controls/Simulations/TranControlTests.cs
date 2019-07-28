using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;

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
            readingContext.EvaluateDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());
            readingContext.SimulationPreparations.Returns(new SimulationPreparations(
                new EntityUpdates(false, 
                    new SimulationEvaluators(new SpiceEvaluator()),
                    new SimulationExpressionContexts(new ExpressionContext())),
                new SimulationsUpdates(new SimulationEvaluators(new SpiceEvaluator()),
                new SimulationExpressionContexts(new ExpressionContext()))));
            // act
            var tranControl = new TransientControl(new ExporterMapper());
            tranControl.Read(control, readingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<Transient>(simulations.First());
            Assert.True(simulations.First().Configurations.Get<TimeConfiguration>().UseIc);
            Assert.Equal(1e-5, simulations.First().Configurations.Get<TimeConfiguration>().Step);
            Assert.Equal(1e-3, simulations.First().Configurations.Get<TimeConfiguration>().FinalTime);
            Assert.Equal(1e-4, simulations.First().Configurations.Get<TimeConfiguration>().MaxStep);
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
            readingContext.EvaluateDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());
            readingContext.SimulationPreparations.Returns(new SimulationPreparations(
                new EntityUpdates(false,
                    new SimulationEvaluators(new SpiceEvaluator()),
                    new SimulationExpressionContexts(new ExpressionContext())),
                new SimulationsUpdates(new SimulationEvaluators(new SpiceEvaluator()),
                    new SimulationExpressionContexts(new ExpressionContext()))));

            // act
            var tranControl = new TransientControl(new ExporterMapper());
            tranControl.Read(control, readingContext);

            // assert
            Assert.Single(simulations);
            Assert.IsType<Transient>(simulations.First());
            Assert.False(simulations.First().Configurations.Get<TimeConfiguration>().UseIc);
            Assert.Equal(1e-5, simulations.First().Configurations.Get<TimeConfiguration>().Step);
            Assert.Equal(1e-3, simulations.First().Configurations.Get<TimeConfiguration>().FinalTime);
            Assert.Equal(1e-4, simulations.First().Configurations.Get<TimeConfiguration>().MaxStep);
        }
    }
}
