using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using System;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Mathematics
{
    public class AngleMathTests
    {
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(3.141592653589793, -3.141592653589793)]
        [InlineData(-3.141592653589793, -3.141592653589793)]
        [InlineData(9.42477796076938, -3.141592653589793)]
        [InlineData(-9.42477796076938, -3.141592653589793)]
        public void WrapSignedUsesHalfOpenInterval(double angle, double expected)
        {
            NumericAssert.Equal(expected, AngleMath.WrapSigned(angle), 1e-15, 1e-15);
        }

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(-1.5707963267948966, 4.71238898038469)]
        [InlineData(6.283185307179586, 0.0)]
        [InlineData(7.853981633974483, 1.5707963267948966)]
        public void WrapPositiveUsesHalfOpenInterval(double angle, double expected)
        {
            NumericAssert.Equal(expected, AngleMath.WrapPositive(angle), 1e-15, 1e-15);
        }

        [Fact]
        public void ShortestDifferenceCrossesPeriodicSeam()
        {
            double from = 179.0 * Math.PI / 180.0;
            double to = -179.0 * Math.PI / 180.0;

            NumericAssert.Equal(
                2.0 * Math.PI / 180.0,
                AngleMath.ShortestDifference(from, to),
                1e-15,
                1e-14);
            NumericAssert.Equal(
                -2.0 * Math.PI / 180.0,
                AngleMath.ShortestDifference(to, from),
                1e-15,
                1e-14);
        }

        [Fact]
        public void WrappingNonFiniteInputDoesNotHideInvalidValue()
        {
            Assert.True(double.IsNaN(AngleMath.WrapSigned(double.PositiveInfinity)));
            Assert.True(double.IsNaN(AngleMath.WrapPositive(double.NegativeInfinity)));
            Assert.True(double.IsNaN(AngleMath.ShortestDifference(0.0, double.NaN)));
        }
    }
}
