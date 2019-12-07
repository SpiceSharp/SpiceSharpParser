using System.Collections.Generic;
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
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    public class ModelReaderTests
    {
        [Fact]
        public void Generate()
        {
            var mapper = Substitute.For<IMapper<IModelGenerator>>();
            mapper.ContainsKey("NPN", false).Returns(true);

            IModelGenerator value;
            mapper.TryGetValue("NPN", false, out value).Returns(
                x =>
                    {
                        x[2] = null;
                        return true;
                    });

            var readingContext = Substitute.For<ICircuitContext>();
            readingContext.NameGenerator.GenerateObjectName(Arg.Any<string>()).Returns(x => x[0].ToString());
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);
            var modelsGenerator = Substitute.For<IModelsGenerator>();
            modelsGenerator.GenerateModel(
                Arg.Any<IModelGenerator>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<ParameterCollection>(),
                Arg.Any<ICircuitContext>()).Returns(x => new BipolarJunctionTransistorModel((string)x[1]));

            // act
            ModelReader reader = new ModelReader(mapper, modelsGenerator);
            var model = new Models.Netlist.Spice.Objects.Model("2Na2222", new ParameterCollection(new List<Parameter>() { new BracketParameter() { Name = "NPN" }}), null);
            reader.Read(model, readingContext);

            // assert
            modelsGenerator.Received().GenerateModel(Arg.Any<IModelGenerator>(), Arg.Any<string>(), "2Na2222", "NPN", Arg.Any<ParameterCollection>(), Arg.Any<ICircuitContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name == "2Na2222"));
        }
    }
}