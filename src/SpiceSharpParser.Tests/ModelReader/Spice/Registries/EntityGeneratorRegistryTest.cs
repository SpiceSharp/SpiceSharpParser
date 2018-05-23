using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Processors.EntityGenerators;
using SpiceSharpParser.ModelReader.Spice.Registries;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Registries
{
    public class EntityGeneratorRegistryTest
    {
        [Fact]
        public void Add()
        {
            // arrange
            var registry = new EntityGeneratorRegistry();
            var entityGenerator = Substitute.For<EntityGenerator>();
            entityGenerator.GetGeneratedSpiceTypes().Returns(new System.Collections.Generic.List<string>() { "r", "v", "i" });

            // act
            registry.Add(entityGenerator);

            // assert
            Assert.Equal(1, registry.Count);
            Assert.True(registry.Supports("r"));
            Assert.True(registry.Supports("v"));
            Assert.True(registry.Supports("i"));
        }
    }
}
