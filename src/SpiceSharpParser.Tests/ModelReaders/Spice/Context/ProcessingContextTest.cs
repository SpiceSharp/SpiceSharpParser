using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharp.Components;
using System.Collections.Generic;
using Xunit;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Context
{
    public class ReadingContextTest
    {
        [Fact]
        public void SetNodeSetVoltageTest()
        {
            // prepare
            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("x+1").Returns(3);

            var simulation = new DC("DC");

            var evaluators = Substitute.For<IEvaluatorsContainer>();
            evaluators.GetSimulationEvaluator(Arg.Any<Simulation>()).Returns(evaluator);
            evaluators.EvaluateDouble("x+1", Arg.Any<Simulation>()).Returns(3);

            var context = new ReadingContext(
                string.Empty,
                new SimulationsParameters(evaluators),
                evaluators,
                Substitute.For<IResultService>(),
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty),
                null,
                null);

            // act
            context.SimulationsParameters.SetNodeSetVoltage("node1", "x+1");
            context.SimulationsParameters.Prepare(simulation);

            // assert
            Assert.Equal(3, simulation.Nodes.NodeSets["node1"]);
        }
    }
}
