using NSubstitute;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
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
               Arg.Any<ICircuitContext>()).Returns(x => new Resistor((string)x[0]));

            var mapper = Substitute.For<IMapper<IComponentGenerator>>();

            Dictionary<string, IComponentGenerator> dict = new Dictionary<string, IComponentGenerator>();
            dict["R"] = generator;
            mapper.GetEnumerator().Returns(dict.GetEnumerator());

            var readingContext = Substitute.For<ICircuitContext>();
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());
            readingContext.NameGenerator.GenerateObjectName(Arg.Any<string>()).Returns(x => x[0].ToString());

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);

            // act
            ComponentReader reader = new ComponentReader(mapper);
            var component = new Models.Netlist.Spice.Objects.Component(
                "Ra1",
                new ParameterCollection(new List<Parameter>()
                {
                    new ValueParameter("0"),
                    new ValueParameter("1"),
                    new ValueParameter("12.3"),
                }), null);

            reader.Read(component, readingContext);

            // assert
            generator.Received().Generate("Ra1", "Ra1", "R", Arg.Any<ParameterCollection>(), Arg.Any<ICircuitContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name == "Ra1"));
        }
    }
}