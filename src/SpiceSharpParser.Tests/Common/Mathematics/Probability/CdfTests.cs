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
            var cdf = new Cdf(pdf, 100);

            // assert
            Assert.Equal(100, cdf.PointsCount);
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
            var cdf = new Cdf(pdf, 100);

            // assert
            Assert.Equal(100, cdf.PointsCount);
        }
    }
}