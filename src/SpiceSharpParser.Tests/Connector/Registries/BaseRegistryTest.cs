using NSubstitute;
using SpiceSharpParser.Connector.Processors.Common;
using SpiceSharpParser.Connector.Registries;
using Xunit;

namespace SpiceSharpParser.Tests.Connector.Registries
{
    public class BaseRegistryTest
    {
        [Fact]
        public void AddSameTypeCountTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<IGenerator>();

            var generator = Substitute.For<IGenerator>();
            generator.TypeName.Returns("test");
            var generator2 = Substitute.For<IGenerator>();
            generator2.TypeName.Returns("test");
            var generator3 = Substitute.For<IGenerator>();
            generator3.TypeName.Returns("test2");
            var generator4 = Substitute.For<IGenerator>();
            generator4.TypeName.Returns("test3");

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
            var baseRegistry = new BaseRegistry<IGenerator>();
            var generator = Substitute.For<IGenerator>();
            generator.TypeName.Returns("test");

            // act
            baseRegistry.Add(generator);

            // assert
            Assert.True(baseRegistry.Supports("test"));
        }

        [Fact]
        public void SupportsNegativeTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<IGenerator>();
            var generator = Substitute.For<IGenerator>();
            generator.TypeName.Returns("test1");

            // act
            baseRegistry.Add(generator);

            // assert
            Assert.False(baseRegistry.Supports("test2"));
        }

        [Fact]
        public void IndexOfTest()
        {
            // arrange
            var baseRegistry = new BaseRegistry<IGenerator>();
            var generator = Substitute.For<IGenerator>();
            generator.TypeName.Returns("test1");

            var generator2 = Substitute.For<IGenerator>();
            generator2.TypeName.Returns("test2");

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
