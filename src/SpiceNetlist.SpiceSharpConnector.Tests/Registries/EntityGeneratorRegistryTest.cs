using NSubstitute;
using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;
using SpiceNetlist.SpiceSharpConnector.Registries;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Registries
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
