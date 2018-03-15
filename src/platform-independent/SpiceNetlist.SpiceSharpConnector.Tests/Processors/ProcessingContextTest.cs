using NSubstitute;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp.Components;
using System.Collections.Generic;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors
{
    public class ProcessingContextTest
    {
        [Fact]
        public void SetParameterWithExpressionTest()
        {
            // prepare
            var evaluator = Substitute.For<IEvaluator>();
            evaluator.EvaluateDouble("a+1", out _).Returns(
                x =>
                {
                    x[1] = new List<string>() { "a" };
                    return 1.1;
                });

            var resultService = Substitute.For<IResultService>();
            var context = new ProcessingContext(string.Empty, evaluator, resultService, new NodeNameGenerator(), new ObjectNameGenerator(string.Empty));

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
            evaluator.EvaluateDouble("1", out _).Returns(1);

            var resultService = Substitute.For<IResultService>();
            var context = new ProcessingContext(string.Empty, 
                evaluator, 
                resultService,
                new NodeNameGenerator(), 
                new ObjectNameGenerator(string.Empty));

            // act
            var resistor = new Resistor("R1");
            context.SetParameter(resistor, "L", "1");

            // assert
            Assert.Equal(1, resistor.ParameterSets.GetParameter("l").Value);
        }
    }
}
