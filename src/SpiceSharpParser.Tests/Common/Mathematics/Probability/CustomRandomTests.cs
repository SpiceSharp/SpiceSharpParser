using NSubstitute;
using SpiceSharpParser.Common.Mathematics.Probability;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Mathematics.Probability
{
    public class CustomRandomTests
    {
        [Fact]
        public void When_NextDouble_Expect_Reference()
        {
            // arrange
            var pdfCurve = new Curve();
            pdfCurve.Add(new Point(-1.0, 1));
            pdfCurve.Add(new Point(0.5, 1));
            pdfCurve.Add(new Point(0.5, 0));
            pdfCurve.Add(new Point(1, 0));
            var pdf = new Pdf(pdfCurve);

            IRandom baseRandom = Substitute.For<IRandom>();
            baseRandom.NextDouble().Returns(0.3);
            var customRandom = new CustomRandom(pdf, baseRandom);

            // act
            var randomNumber = customRandom.NextDouble();

            // assert
            Assert.Equal(-0.55, randomNumber);
        }
    }
}
