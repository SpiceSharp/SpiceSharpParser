using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Registries
{
    public class ExporterRegistryTest
    {
        [Fact]
        public void AddComplexExporter()
        {
            // arrange
            var registry = new ExporterRegistry();
            var exporter = Substitute.For<Exporter>();
            exporter.GetSupportedTypes().Returns(new System.Collections.Generic.List<string>() { "v", "v1", "v2" });

            // act
            registry.Add(exporter);

            // assert
            Assert.Equal(1, registry.Count);
            Assert.True(registry.Supports("v"));
            Assert.True(registry.Supports("v1"));
            Assert.True(registry.Supports("v2"));
        }
    }
}
