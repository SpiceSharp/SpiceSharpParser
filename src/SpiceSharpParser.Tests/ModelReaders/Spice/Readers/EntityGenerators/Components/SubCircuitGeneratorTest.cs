using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components
{
    public class SubCircuitGeneratorTest
    {
        [Fact]
        public void GenerateBasicTest()
        {
            // prepare
            var context = Substitute.For<IReadingContext>();
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
                        new Model() { Name = "m1" },
                        new Control() { Name = "param", Parameters = new ParameterCollection() },
                        new Control() { Name = "save" }
                    }
                }
            });
            context.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }));
            context.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            IReadingContext parent = null;
            context.Parent.Returns<IReadingContext>(parent);

            var childrenContexts = new List<IReadingContext>();
            context.Children.Returns(childrenContexts);

            var componentReader = Substitute.For<IComponentReader>();
            var modelReader = Substitute.For<IModelReader>();
            var controlReader = Substitute.For<IControlReader>();
            var subcircuitDefinitionReader = Substitute.For<ISubcircuitDefinitionReader>();

            // act
            var generator = new SubCircuitGenerator(componentReader, modelReader, controlReader, subcircuitDefinitionReader);
            generator.Generate(new SpiceSharp.StringIdentifier("x1"), "x1", "x", parameters, context);

            // assert
            componentReader.Received().Read(Arg.Is<Component>(c => c.Name == "R1"), Arg.Any<IReadingContext>());
            modelReader.Received().Read(Arg.Is<Model>(c => c.Name == "m1"), Arg.Any<IReadingContext>());
            controlReader.Received().Read(Arg.Is<Control>(c => c.Name == "param"), Arg.Any<IReadingContext>());
            controlReader.DidNotReceive().Read(Arg.Is<Control>(c => c.Name == "save"), Arg.Any<IReadingContext>());

            Assert.Single(childrenContexts);
        }
    }
}
