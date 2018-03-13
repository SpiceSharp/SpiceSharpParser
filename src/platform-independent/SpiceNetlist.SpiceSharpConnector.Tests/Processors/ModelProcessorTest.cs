using NSubstitute;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;
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
                "npn",
                Arg.Any<ParameterCollection>(),
                Arg.Any<ProcessingContextBase>()).Returns(new BipolarJunctionTransistorModel(new Identifier("test")));

            var registry = Substitute.For<IEntityGeneratorRegistry>();
            registry.Supports("npn").Returns(true);
            registry.Get("npn").Returns(generator);

            var processingContext = Substitute.For<ProcessingContextBase>();
            processingContext.NameGenerator.Returns(new NameGenerator("path"));

            var adder = Substitute.For<INetlistAdder>();
            processingContext.Adder.Returns(adder);

            // act
            ModelProcessor processor = new ModelProcessor(registry);
            var model = new SpiceObjects.Model() { Name = "2N2222", Parameters = new ParameterCollection() { new BracketParameter() { Name = "NPN" } } };
            processor.Process(model, processingContext);

            //assert
            adder.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.Name == "test"));
        }
    }
}
