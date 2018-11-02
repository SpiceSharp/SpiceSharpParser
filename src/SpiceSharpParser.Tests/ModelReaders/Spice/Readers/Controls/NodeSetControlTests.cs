using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class NodeSetControlTests
    {
        [Fact]
        public void Read()
        {
            // prepare
            var control = new Control()
            {
                Name = "nodeset",
                Parameters = new ParameterCollection()
                {
                    new AssignmentParameter()
                    {
                        Name = "V",
                        Arguments = new List<string>()
                        {
                            "input"
                        },
                        Value = "12"
                    },
                    new AssignmentParameter()
                    {
                        Name = "V",
                        Arguments = new List<string>()
                        {
                            "x"
                        },
                        Value = "13"
                    }
                }
            };

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.CaseSensitivity = new SpiceNetlistCaseSensitivitySettings();
            readingContext.SimulationsParameters.Returns(Substitute.For<ISimulationsParameters>());
            readingContext.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }, false));
            // act
            var nodeSetControl = new NodeSetControl();
            nodeSetControl.Read(control, readingContext);

            // assert
            readingContext.SimulationsParameters.Received().SetNodeSetVoltage(Arg.Any<SimulationExpressionContexts>(), "input", "12");
            readingContext.SimulationsParameters.Received().SetNodeSetVoltage(Arg.Any<SimulationExpressionContexts>(), "x", "13");
        }
    }
}
