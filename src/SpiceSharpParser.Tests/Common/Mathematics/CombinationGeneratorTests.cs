using SpiceSharpParser.Common.Mathematics.Combinatorics;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Mathematics
{
    public class CombinationGeneratorTests
    {
        [Fact]
        public void GenerateReturnsRightNumberOfCombinations()
        {
            CombinationGenerator generator = new CombinationGenerator();
            var result = generator.Generate(112, 2);
            Assert.Equal(112, result.Count);
        }

        [Fact]
        public void GenerateRightCombinationsForNEqualsTwo()
        {
            CombinationGenerator generator = new CombinationGenerator();
            var result = generator.Generate(12, 2);
            Assert.Equal(12, result.Count);
            Assert.Equal(new int[] { }, result[0]);
            Assert.Equal(new[] { 1 }, result[1]);
            Assert.Equal(new[] { 2 }, result[2]);
            Assert.Equal(new[] { 1, 1 }, result[3]);
            Assert.Equal(new[] { 1, 2 }, result[4]);
            Assert.Equal(new[] { 2, 2 }, result[5]);
            Assert.Equal(new[] { 1, 1, 1 }, result[6]);
            Assert.Equal(new[] { 1, 1, 2 }, result[7]);
            Assert.Equal(new[] { 1, 2, 2 }, result[8]);
            Assert.Equal(new[] { 2, 2, 2 }, result[9]);
            Assert.Equal(new[] { 1, 1, 1, 1 }, result[10]);
            Assert.Equal(new[] { 1, 1, 1, 2 }, result[11]);
        }

        [Fact]
        public void GenerateRightCombinationsForNEqualsThree()
        {
            CombinationGenerator generator = new CombinationGenerator();
            var result = generator.Generate(25, 3);
            Assert.Equal(25, result.Count);
            Assert.Equal(new int[] { }, result[0]);
            Assert.Equal(new[] { 1 }, result[1]);
            Assert.Equal(new[] { 2 }, result[2]);
            Assert.Equal(new[] { 3 }, result[3]);
            Assert.Equal(new[] { 1, 1 }, result[4]);
            Assert.Equal(new[] { 1, 2 }, result[5]);
            Assert.Equal(new[] { 1, 3 }, result[6]);
            Assert.Equal(new[] { 2, 2 }, result[7]);
            Assert.Equal(new[] { 2, 3 }, result[8]);
            Assert.Equal(new[] { 3, 3 }, result[9]);
            Assert.Equal(new[] { 1, 1, 1 }, result[10]);
            Assert.Equal(new[] { 1, 1, 2 }, result[11]);
            Assert.Equal(new[] { 1, 1, 3 }, result[12]);
            Assert.Equal(new[] { 1, 2, 2 }, result[13]);
            Assert.Equal(new[] { 1, 2, 3 }, result[14]);
            Assert.Equal(new[] { 1, 3, 3 }, result[15]);
            Assert.Equal(new[] { 2, 2, 2 }, result[16]);
            Assert.Equal(new[] { 2, 2, 3 }, result[17]);
            Assert.Equal(new[] { 2, 3, 3 }, result[18]);
            Assert.Equal(new[] { 3, 3, 3 }, result[19]);
        }
    }
}