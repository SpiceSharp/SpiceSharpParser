using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharp.Components;
using System.Collections.Generic;
using Xunit;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Context
{
    public class ProcessingContextTest
    {
        [Fact]
        public void SetParameterWithExpressionTest()
        {
            // prepare
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble("a+1").Returns(
                x =>
                {
                    return 1.1;
                });

            var resultService = Substitute.For<IResultService>();
            var context = new ProcessingContext(string.Empty, evaluator, resultService, new MainCircuitNodeNameGenerator(new string[] { }), new ObjectNameGenerator(string.Empty));

            // act
            var resistor = new Resistor("R1");
            context.SetParameter(resistor, "resistance", "a+1");

            // assert 
            Assert.Equal(1.1, resistor.ParameterSets.GetParameter("resistance").Value);
        }

        [Fact]
        public void SetParameterCaseTest()
        {
            // prepare
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble("1").Returns(1);

            var resultService = Substitute.For<IResultService>();
            var context = new ProcessingContext(string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var resistor = new Resistor("R1");
            context.SetParameter(resistor, "L", "1");

            // assert
            Assert.Equal(1, resistor.ParameterSets.GetParameter("l").Value);
        }

        [Fact]
        public void SetUnkownParameterTest()
        {
            // prepare
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble("1").Returns(1);

            var resultService = Substitute.For<IResultService>();
            var context = new ProcessingContext(string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var resistor = new Resistor("R1");
            Assert.False(context.SetParameter(resistor, "uknown", "1"));
        }

        [Fact]
        public void SetNodeSetVoltageTest()
        {
            // prepare
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble("x+1").Returns(3);

            var simulations = new List<Simulation>();
            var simulation = new DC("DC");
            simulations.Add(simulation);

            var resultService = Substitute.For<IResultService>();
            resultService.SimulationConfiguration.Returns(new SimulationConfiguration());
            resultService.Simulations.Returns(simulations);

            var context = new ProcessingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            context.SetNodeSetVoltage("node1", "x+1");

            // assert
            Assert.Equal(3, simulation.Nodes.NodeSets["node1"]);
        }
    }
}
