using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    public class ModelReaderTest
    {
        [Fact]
        public void GenerateTest()
        {
            // arrange
            var generator = Substitute.For<ModelGenerator>();
            generator.Generate(
                Arg.Any<StringIdentifier>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<ParameterCollection>(),
                Arg.Any<IReadingContext>()).Returns(x => new BipolarJunctionTransistorModel((StringIdentifier)x[0]));

            var registry = Substitute.For<IMapper<ModelGenerator>>();
            registry.Contains("npn").Returns(true);
            registry.Get("npn").Returns(generator);

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }));
            readingContext.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);

            // act
            ModelReader reader = new ModelReader(registry);
            var model = new Models.Netlist.Spice.Objects.Model() { Name = "2Na2222", Parameters = new ParameterCollection() { new BracketParameter() { Name = "NPN" } } };
            reader.Read(model, readingContext);

            //assert
            generator.Received().Generate(new StringIdentifier("2Na2222"), "2Na2222", "npn", Arg.Any<ParameterCollection>(), Arg.Any<IReadingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.ToString() == "2Na2222"));
        }
    }
}
