using NSubstitute;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors;
using SpiceSharpParser.Connector.Processors.EntityGenerators;
using SpiceSharpParser.Connector.Registries;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using Xunit;

namespace SpiceSharpParser.Tests.Connector.Processors
{
    public class ModelProcessorTest
    {
        [Fact]
        public void GenerateTest()
        {
            // arrange
            var generator = Substitute.For<EntityGenerator>();
            generator.Generate(
                Arg.Any<StringIdentifier>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<ParameterCollection>(),
                Arg.Any<IProcessingContext>()).Returns(x => new BipolarJunctionTransistorModel((StringIdentifier)x[0]));

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
            var model = new Model.SpiceObjects.Model() { Name = "2Na2222", Parameters = new ParameterCollection() { new BracketParameter() { Name = "NPN" } } };
            processor.Process(model, processingContext);

            //assert
            generator.Received().Generate(new StringIdentifier("2Na2222"), "2Na2222", "npn", Arg.Any<ParameterCollection>(), Arg.Any<IProcessingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.ToString() == "2Na2222"));
        }
    }
}
