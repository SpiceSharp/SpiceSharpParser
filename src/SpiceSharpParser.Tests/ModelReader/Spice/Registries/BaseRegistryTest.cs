using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Registries
{
    public class BaseRegistryTest
    {
        [Fact]
        public void AddSameTypeCountTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<ISpiceObjectReader>();

            var generator = Substitute.For<ISpiceObjectReader>();
            generator.SpiceName.Returns("test");
            var generator2 = Substitute.For<ISpiceObjectReader>();
            generator2.SpiceName.Returns("test");
            var generator3 = Substitute.For<ISpiceObjectReader>();
            generator3.SpiceName.Returns("test2");
            var generator4 = Substitute.For<ISpiceObjectReader>();
            generator4.SpiceName.Returns("test3");

            // act
            baseRegistry.Add(generator);
            baseRegistry.Add(generator2);
            baseRegistry.Add(generator3);
            baseRegistry.Add(generator4);

            // assert
            Assert.Equal(3, baseRegistry.Count);
        }

        [Fact]
        public void SupportsPositiveTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<ISpiceObjectReader>();
            var generator = Substitute.For<ISpiceObjectReader>();
            generator.SpiceName.Returns("test");

            // act
            baseRegistry.Add(generator);

            // assert
            Assert.True(baseRegistry.Supports("test"));
        }

        [Fact]
        public void SupportsNegativeTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<ISpiceObjectReader>();
            var generator = Substitute.For<ISpiceObjectReader>();
            generator.SpiceName.Returns("test1");

            // act
            baseRegistry.Add(generator);

            // assert
            Assert.False(baseRegistry.Supports("test2"));
        }

        [Fact]
        public void IndexOfTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<ISpiceObjectReader>();
            var generator = Substitute.For<ISpiceObjectReader>();
            generator.SpiceName.Returns("test1");

            var generator2 = Substitute.For<ISpiceObjectReader>();
            generator2.SpiceName.Returns("test2");

            // act
            baseRegistry.Add(generator);
            baseRegistry.Add(generator2);

            // assert
            Assert.Equal(0, baseRegistry.IndexOf("test1"));
            Assert.Equal(1, baseRegistry.IndexOf("test2"));

            Assert.Equal(-1, baseRegistry.IndexOf("test3"));
        }
    }
}
