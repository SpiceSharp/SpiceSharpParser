using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    public class ComponentReaderTests
    {
        [Fact]
        public void Generate()
        {
            // arrange
            var generator = Substitute.For<IComponentGenerator>();
            generator.Generate(
               Arg.Any<string>(),
               Arg.Any<string>(),
               Arg.Any<string>(),
               Arg.Any<ParameterCollection>(),
               Arg.Any<IReadingContext>()).Returns(x => new Resistor((string)x[0]));

            var mapper = Substitute.For<IMapper<IComponentGenerator>>();
            mapper.ContainsKey("R", false).Returns(true);
            mapper.GetValue("R", false).Returns(generator);

            IComponentGenerator value;
            mapper.TryGetValue("R", false, out value).Returns(
                x =>
                    {
                        x[2] = generator;
                        return true;
                    });

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }, true));
            readingContext.ComponentNameGenerator.Returns(new ObjectNameGenerator(string.Empty));
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);

            // act
            ComponentReader reader = new ComponentReader(mapper);
            var component = new Models.Netlist.Spice.Objects.Component() { Name = "Ra1", PinsAndParameters = new ParameterCollection() { new ValueParameter("0"), new ValueParameter("1"), new ValueParameter("12.3") } };
            reader.Read(component, readingContext);

            // assert
            generator.Received().Generate("Ra1", "Ra1", "R", Arg.Any<ParameterCollection>(), Arg.Any<IReadingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name== "Ra1"));
        }
    }
}
