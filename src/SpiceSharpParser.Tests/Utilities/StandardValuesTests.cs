using SpiceSharpParser.Utilities;
using Xunit;

namespace SpiceSharpParser.Tests.Utilities
{
    public class StandardValuesTests
    {
        [Theory]
        [InlineData(1000, 1000)]      // Exact E24 value
        [InlineData(1500, 1500)]      // Exact E24 value
        [InlineData(1592, 1600)]      // Nearest above (1600 is closer than 1500)
        [InlineData(1750, 1800)]      // Nearest above
        [InlineData(4500, 4700)]      // Between 4.3k and 4.7k
        [InlineData(99, 100)]         // Cross-decade
        [InlineData(0.047, 0.047)]    // Small value
        [InlineData(10e6, 10e6)]      // Large value
        public void NearestE24_ReturnsCorrectValue(double input, double expected)
        {
            double result = StandardValues.NearestE24(input);
            Assert.Equal(expected, result, 4);
        }

        [Theory]
        [InlineData(1000, 1000)]
        [InlineData(1500, 1500)]
        [InlineData(2000, 2200)]      // E12 has no 2.0
        [InlineData(5000, 4700)]      // Nearest is 4.7k in E12
        public void NearestE12_ReturnsCorrectValue(double input, double expected)
        {
            double result = StandardValues.NearestE12(input);
            Assert.Equal(expected, result, 4);
        }

        [Fact]
        public void BracketE24_ReturnsBracketingValues()
        {
            var (below, above) = StandardValues.BracketE24(1592);
            Assert.Equal(1500, below, 4);
            Assert.Equal(1600, above, 4);
        }

        [Fact]
        public void BracketE24_ExactValue_ReturnsSameForBoth()
        {
            var (below, above) = StandardValues.BracketE24(1000);
            Assert.Equal(1000, below, 4);
            Assert.Equal(1000, above, 4);
        }

        [Fact]
        public void GetValuesInRange_ReturnsCorrectSet()
        {
            var values = StandardValues.GetValuesInRange(1000, 3300, StandardValues.E24Multipliers);
            Assert.Contains(1000.0, values);
            Assert.Contains(1500.0, values);
            Assert.Contains(2200.0, values);
            Assert.Contains(3300.0, values);
            Assert.DoesNotContain(910.0, values);
            Assert.DoesNotContain(3600.0, values);
        }

        [Fact]
        public void NearestE24_ThrowsForNegativeValue()
        {
            Assert.Throws<System.ArgumentException>(() => StandardValues.NearestE24(-100));
        }

        [Fact]
        public void NearestE96_ReturnsCloseValue()
        {
            double result = StandardValues.NearestE96(1234);
            // E96 has 1.24k
            Assert.InRange(result, 1200, 1250);
        }
    }
}
