using SpiceSharpParser.Common.Mathematics.Probability;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Mathematics.Probability
{
    public class CdfTests
    {
        [Fact]
        public void When_ConstPdf_Expect_Reference()
        {
            // arrange
            var curve = new Curve();
            curve.Add(new Point(-1, 0.5));
            curve.Add(new Point(1, 0.5));
            var pdf = new Pdf(curve);

            // act
            var cdf = new Cdf(pdf);

            // assert
            Assert.Equal(2, cdf.PointsCount);
            Assert.Equal(-1, cdf[0].X);
            Assert.Equal(0, cdf[0].Y);
            Assert.Equal(1, cdf[1].X);
            Assert.Equal(1, cdf[1].Y);
        }

        [Fact]
        public void When_CustomPdf_Expect_Reference()
        {
            // arrange
            var curve = new Curve();
            curve.Add(new Point(-1, 3));
            curve.Add(new Point(0, 2));
            curve.Add(new Point(1, 3));

            var pdf = new Pdf(curve);

            // act
            var cdf = new Cdf(pdf);

            // assert
            Assert.Equal(3, cdf.PointsCount);
            Assert.Equal(-1, cdf[0].X);
            Assert.Equal(0, cdf[0].Y);

            Assert.Equal(0, cdf[1].X);
            Assert.Equal(2.5 / 5.0, cdf[1].Y);

            Assert.Equal(1, cdf[2].X);
            Assert.Equal(1.0, cdf[2].Y);
        }
    }
}
