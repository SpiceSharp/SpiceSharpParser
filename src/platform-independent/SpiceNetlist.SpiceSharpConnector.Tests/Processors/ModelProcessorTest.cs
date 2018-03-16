using NSubstitute;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Registries;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors
{
    public class ModelProcessorTest
    {
        [Fact]
        public void GenerateTest()
        {
            // arrange
            var generator = Substitute.For<EntityGenerator>();
            generator.Generate(
                Arg.Any<Identifier>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<ParameterCollection>(),
                Arg.Any<IProcessingContext>()).Returns(x => new BipolarJunctionTransistorModel((Identifier)x[0]));

            var registry = Substitute.For<IEntityGeneratorRegistry>();
            registry.Supports("npn").Returns(true);
            registry.Get("npn").Returns(generator);

            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.NodeNameGenerator.Returns(new NodeNameGenerator());
            processingContext.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            var resultService = Substitute.For<IResultService>();
            processingContext.Result.Returns(resultService);

            // act
            ModelProcessor processor = new ModelProcessor(registry);
            var model = new SpiceObjects.Model() { Name = "2Na2222", Parameters = new ParameterCollection() { new BracketParameter() { Name = "NPN" } } };
            processor.Process(model, processingContext);

            //assert
            generator.Received().Generate(new Identifier("2Na2222"), "2Na2222", "npn", Arg.Any<ParameterCollection>(), Arg.Any<IProcessingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.Name == "2Na2222"));
        }
    }
}
