using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Xunit;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors.Controls.Simulations
{
    public class NodeSetControlTest
    {
        [Fact]
        public void Process()
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

            var processingContext = Substitute.For<IProcessingContext>();

            // act
            var nodeSetControl = new NodeSetControl();
            nodeSetControl.Process(control, processingContext);

            // assert
            processingContext.Received().SetNodeSetVoltage("input", "12");
            processingContext.Received().SetNodeSetVoltage("x", "13");
        }
    }
}
