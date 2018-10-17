using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using Xunit;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Context
{
    public class ReadingContextTests
    {
        [Fact]
        public void SetNodeSetVoltage()
        {
            // prepare
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble("x+1").Returns(3);

            var simulation = new DC("DC");

            var evaluators = Substitute.For<ISimulationEvaluatorsContainer>();
            evaluators.GetSimulationEvaluator(Arg.Any<Simulation>()).Returns(evaluator);
            evaluators.EvaluateDouble("x+1", Arg.Any<Simulation>()).Returns(3);

            var context = new ReadingContext(
                string.Empty,
                new SimulationsParameters(evaluators),
                evaluators,
                Substitute.For<IResultService>(),
                new MainCircuitNodeNameGenerator(new string[] { }, true),
                new ObjectNameGenerator(string.Empty),
                new ObjectNameGenerator(string.Empty),
                null,
                null,
                new SpiceNetlistCaseSensitivitySettings());

            // act
            context.SimulationsParameters.SetNodeSetVoltage("node1", "x+1");
            context.SimulationsParameters.Prepare(simulation);

            // assert
            Assert.Equal(3, simulation.Configurations.Get<BaseConfiguration>().Nodesets["node1"]);
        }
    }
}
