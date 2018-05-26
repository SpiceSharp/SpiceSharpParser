using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.EntityGenerators.Components;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors.EntityGenerators.Components
{
    public class SubCircuitGeneratorTest
    {
        [Fact]
        public void GenerateBasicTest()
        {
            // prepare
            var context = Substitute.For<IProcessingContext>();
            var parameters = new ParameterCollection
            {
                new IdentifierParameter("net2"),
                new IdentifierParameter("net3"),
                new WordParameter("lcFilter"),
                new AssignmentParameter() { Name = "L", Value = "0" },
                new AssignmentParameter() { Name = "C", Value = "100" }
            };

            context.AvailableSubcircuits.Returns(new System.Collections.Generic.List<SubCircuit>()
            {
                new SubCircuit()
                {
                    Name = "lcFilter",
                    Statements = new Statements()
                    {
                        new Component() { Name = "R1" },
                        new SpiceSharpParser.Model.Netlist.Spice.Objects.Model() { Name = "m1" },
                        new Control() { Name = "param", Parameters = new ParameterCollection() },
                        new Control() { Name = "save" }
                    }
                }
            });
            context.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }));
            context.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            var childrenContexts = new List<IProcessingContext>();
            context.Children.Returns(childrenContexts);

            var componentProcessor = Substitute.For<IComponentProcessor>();
            var modelProcessor = Substitute.For<IModelProcessor>();
            var controlProcessor = Substitute.For<IControlProcessor>();
            var subcircuitDefinitionProcessor = Substitute.For<ISubcircuitDefinitionProcessor>();

            // act
            var generator = new SubCircuitGenerator(componentProcessor, modelProcessor, controlProcessor, subcircuitDefinitionProcessor);
            generator.Generate(new SpiceSharp.StringIdentifier("x1"), "x1", "x", parameters, context);

            // assert
            componentProcessor.Received().Process(Arg.Is<Component>(c => c.Name == "R1"), Arg.Any<IProcessingContext>());
            modelProcessor.Received().Process(Arg.Is<SpiceSharpParser.Model.Netlist.Spice.Objects.Model>(c => c.Name == "m1"), Arg.Any<IProcessingContext>());
            controlProcessor.Received().Process(Arg.Is<Control>(c => c.Name == "param"), Arg.Any<IProcessingContext>());
            controlProcessor.DidNotReceive().Process(Arg.Is<Control>(c => c.Name == "save"), Arg.Any<IProcessingContext>());

            Assert.Single(childrenContexts);
        }
    }
}
