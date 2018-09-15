using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharp.Components;
using System.Collections.Generic;
using Xunit;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Context
{
    public class ReadingContextTest
    {
        [Fact]
        public void SetParameterWithExpressionTest()
        {
            // prepare
            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("a+1").Returns(
                x =>
                {
                    return 1.1;
                });

            var resultService = Substitute.For<IResultService>();
            var context = new ReadingContext(string.Empty, Substitute.For<ISimulationContexts>(), evaluator, resultService, new MainCircuitNodeNameGenerator(new string[] { }), new ObjectNameGenerator(string.Empty), null);

            // act
            var resistor = new Resistor("R1");
            context.SetParameter(resistor, "resistance", "a+1");

            // assert 
            Assert.Equal(1.1, resistor.ParameterSets.GetParameter<double>("resistance").Value);
        }

        [Fact]
        public void SetParameterCaseTest()
        {
            // prepare
            var readingEvaluator = Substitute.For<ISpiceEvaluator>();
            readingEvaluator.EvaluateDouble("1").Returns(1);

            var resultService = Substitute.For<IResultService>();
            var context = new ReadingContext(string.Empty,
                Substitute.For<ISimulationContexts>(),
                readingEvaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty),
                null);

            // act
            var resistor = new Resistor("R1");
            context.SetParameter(resistor, "L", "1");

            // assert
            Assert.Equal(1, resistor.ParameterSets.GetParameter<double>("l").Value);
        }

        [Fact]
        public void SetUnkownParameterTest()
        {
            // prepare
            var readingEvaluator = Substitute.For<ISpiceEvaluator>();
            readingEvaluator.EvaluateDouble("1").Returns(1);

            var resultService = Substitute.For<IResultService>();
            var context = new ReadingContext(string.Empty,
                Substitute.For<ISimulationContexts>(),
                readingEvaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty),
                null);

            // act
            var resistor = new Resistor("R1");
            Assert.False(context.SetParameter(resistor, "uknown", "1"));
        }

        [Fact]
        public void SetNodeSetVoltageTest()
        {
            // prepare
            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("x+1").Returns(3);

            var simulations = new List<Simulation>();
            var simulation = new DC("DC");
            simulations.Add(simulation);

            var resultService = Substitute.For<IResultService>();
            resultService.SimulationConfiguration.Returns(new SimulationConfiguration());
            resultService.Simulations.Returns(simulations);

            var context = new ReadingContext(
                string.Empty,
                Substitute.For<ISimulationContexts>(),
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty),
                null);

            // act
            context.SetNodeSetVoltage("node1", "x+1");

            // assert
            Assert.Equal(3, simulation.Nodes.NodeSets["node1"]);
        }
    }
}
