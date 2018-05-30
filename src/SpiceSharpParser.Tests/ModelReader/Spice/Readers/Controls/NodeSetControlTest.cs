using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Xunit;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Readers.Controls.Simulations
{
    public class NodeSetControlTest
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

            // act
            var nodeSetControl = new NodeSetControl();
            nodeSetControl.Read(control, readingContext);

            // assert
            readingContext.Received().SetNodeSetVoltage("input", "12");
            readingContext.Received().SetNodeSetVoltage("x", "13");
        }
    }
}
