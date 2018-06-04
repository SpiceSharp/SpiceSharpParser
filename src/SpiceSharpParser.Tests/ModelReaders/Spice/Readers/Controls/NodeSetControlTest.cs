using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Xunit;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
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
