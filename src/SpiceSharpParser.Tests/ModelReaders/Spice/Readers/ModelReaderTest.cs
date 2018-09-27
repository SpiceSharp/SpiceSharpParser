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

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    public class ModelReaderTest
    {
        [Fact]
        public void GenerateTest()
        {
            var mapper = Substitute.For<IMapper<IModelGenerator>>();
            mapper.Contains("npn").Returns(true);

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }));
            readingContext.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);
            var modelsGenerator = Substitute.For<IModelsGenerator>();
            modelsGenerator.GenerateModel(
                Arg.Any<IModelGenerator>(),
                Arg.Any<Identifier>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<ParameterCollection>(),
                Arg.Any<IReadingContext>()).Returns(x => new BipolarJunctionTransistorModel((Identifier)x[1]));

            // act
            ModelReader reader = new ModelReader(mapper, modelsGenerator);
            var model = new Models.Netlist.Spice.Objects.Model() { Name = "2Na2222", Parameters = new ParameterCollection() { new BracketParameter() { Name = "NPN" } } };
            reader.Read(model, readingContext);

            //assert
            modelsGenerator.Received().GenerateModel(Arg.Any<IModelGenerator>(), Arg.Any<Identifier>(), "2Na2222", "npn", Arg.Any<ParameterCollection>(), Arg.Any<IReadingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.ToString() == "2Na2222"));
        }
    }
}
