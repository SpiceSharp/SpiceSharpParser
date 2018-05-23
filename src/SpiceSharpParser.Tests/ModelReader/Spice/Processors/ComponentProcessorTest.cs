using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Processors;
using SpiceSharpParser.ModelReader.Spice.Processors.EntityGenerators;
using SpiceSharpParser.ModelReader.Spice.Registries;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors
{
    public class ComponentProcessorTest
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
               Arg.Any<IProcessingContext>()).Returns(x => new Resistor((StringIdentifier)x[0]));

            var registry = Substitute.For<IEntityGeneratorRegistry>();
            registry.Supports("r").Returns(true);
            registry.Get("r").Returns(generator);

            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.NodeNameGenerator.Returns(new MainCircuitNodeNameGenerator(new string[] { }));
            processingContext.ObjectNameGenerator.Returns(new ObjectNameGenerator(string.Empty));

            var resultService = Substitute.For<IResultService>();
            processingContext.Result.Returns(resultService);

            // act
            ComponentProcessor processor = new ComponentProcessor(registry);
            var component = new Model.Spice.Objects.Component() { Name = "Ra1", PinsAndParameters = new ParameterCollection() { new ValueParameter("0"), new ValueParameter("1"), new ValueParameter("12.3") } };
            processor.Process(component, processingContext);

            // assert
            generator.Received().Generate(new StringIdentifier("Ra1"), "Ra1", "r", Arg.Any<ParameterCollection>(), Arg.Any<IProcessingContext>());
            resultService.Received().AddEntity(Arg.Is<Entity>((Entity e) => e.Name.ToString() == "Ra1"));
        }
    }
}
