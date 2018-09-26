using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    public class ComponentReaderTest
    {
        [Fact]
        public void GenerateTest()
        {
            // arrange
            var generator = Substitute.For<IComponentGenerator>();
            generator.Generate(
               Arg.Any<StringIdentifier>(),
               Arg.Any<string>(),
               Arg.Any<string>(),
               Arg.Any<ParameterCollection>(),
               Arg.Any<IReadingContext>()).Returns(x => new Resistor((StringIdentifier)x[0]));

            var mapper = Substitute.For<IMapper<IComponentGenerator>>();
            mapper.Contains("r").Returns(true);
            mapper.Get("r").Returns(generator);

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }));
            readingContext.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);

            // act
            ComponentReader reader = new ComponentReader(mapper);
            var component = new Models.Netlist.Spice.Objects.Component() { Name = "Ra1", PinsAndParameters = new ParameterCollection() { new ValueParameter("0"), new ValueParameter("1"), new ValueParameter("12.3") } };
            reader.Read(component, readingContext);

            // assert
            generator.Received().Generate(new StringIdentifier("Ra1"), "Ra1", "r", Arg.Any<ParameterCollection>(), Arg.Any<IReadingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.ToString() == "Ra1"));
        }
    }
}
