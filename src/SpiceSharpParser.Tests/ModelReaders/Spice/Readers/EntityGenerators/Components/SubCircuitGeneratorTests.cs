using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using SpiceSharpParser.Common;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components
{
    public class SubCircuitGeneratorTests
    {
        [Fact]
        public void GenerateBasic()
        {
            // prepare
            var context = Substitute.For<IReadingContext>();
            context.CaseSensitivity = new SpiceNetlistCaseSensitivitySettings();

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
            context.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }, true));
            context.ComponentNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            IReadingContext parent = null;
            context.Parent.Returns<IReadingContext>(parent);

            var childrenContexts = new List<IReadingContext>();
            context.Children.Returns(childrenContexts);
            context.StatementsReader = Substitute.For<ISpiceStatementsReader>();

            // act
            var generator = new SubCircuitGenerator();
            generator.Generate("x1", "x1", "x", parameters, context);

            // assert
            context.StatementsReader.Received().Read(Arg.Is<Component>(c => c.Name == "R1"), Arg.Any<IReadingContext>());
            context.StatementsReader.Received().Read(Arg.Is<Model>(c => c.Name == "m1"), Arg.Any<IReadingContext>());
            context.StatementsReader.Received().Read(Arg.Is<Control>(c => c.Name == "param"), Arg.Any<IReadingContext>());
            context.StatementsReader.DidNotReceive().Read(Arg.Is<Control>(c => c.Name == "save"), Arg.Any<IReadingContext>());

            Assert.Single(childrenContexts);
        }
    }
}
