using NSubstitute;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Controls.Simulations;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using System.Collections.Generic;
using Xunit;
using SpiceSharpParser.Connector.Processors.Controls;

namespace SpiceSharpParser.Tests.Connector.Processors.Controls.Simulations
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
