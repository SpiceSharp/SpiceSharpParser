using NSubstitute;
using SpiceSharpParser.Common.Mathematics.Probability;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Mathematics.Probability
{
    public class CustomRandomNumberProviderTests
    {
        [Fact]
        public void When_NextSignedDouble_Expect_Reference()
        {
            // arrange
            var pdfCurve = new Curve();
            pdfCurve.Add(new Point(-1.0, 1));
            pdfCurve.Add(new Point(0, 1));
            var pdf = new Pdf(pdfCurve);

            IRandomNumberProvider baseRandom = Substitute.For<IRandomNumberProvider>();
            baseRandom.NextDouble().Returns(0.5);
            var customRandom = new CustomRandomNumberProvider(new Cdf(pdf, 10), baseRandom);

            // act
            var randomNumber = customRandom.NextSignedDouble();

            // assert
            Assert.Equal(-0.5, randomNumber);
        }

        [Fact]
        public void When_NextDouble_Expect_Reference()
        {
            // arrange
            var pdfCurve = new Curve();
            pdfCurve.Add(new Point(-1.0, 1));
            pdfCurve.Add(new Point(1, 1));
            var pdf = new Pdf(pdfCurve);

            IRandomNumberProvider baseRandom = Substitute.For<IRandomNumberProvider>();
            baseRandom.NextDouble().Returns(0.3);
            var customRandom = new CustomRandomNumberProvider(new Cdf(pdf, 10), baseRandom);

            // act
            var randomNumber = customRandom.NextDouble();

            // assert
            Assert.Equal(0.3, randomNumber);
        }

        [Fact]
        public void When_NextDouble_Zeros_Expect_Reference()
        {
            // arrange
            var pdfCurve = new Curve();
            pdfCurve.Add(new Point(-1.0, 0));
            pdfCurve.Add(new Point(0, 0));
            pdfCurve.Add(new Point(1, 1));
            var pdf = new Pdf(pdfCurve);

            IRandomNumberProvider baseRandom = Substitute.For<IRandomNumberProvider>();
            baseRandom.NextDouble().Returns(0);
            var customRandom = new CustomRandomNumberProvider(new Cdf(pdf, 10), baseRandom);

            // act
            var randomNumber = customRandom.NextDouble();

            // assert
            Assert.Equal(0, randomNumber);
        }

        [Fact]
        public void When_NextSignedDouble_Uniform_Expect_Reference()
        {
            // arrange
            var pdfCurve = new Curve();
            pdfCurve.Add(new Point(-1.0, 1));
            pdfCurve.Add(new Point(1.0, 1));
            var pdf = new Pdf(pdfCurve);

            IRandomNumberProvider baseRandom = Substitute.For<IRandomNumberProvider>();
            baseRandom.NextDouble().Returns(0.5);
            var customRandom = new CustomRandomNumberProvider(new Cdf(pdf, 10), baseRandom);

            // act
            var randomNumber = customRandom.NextSignedDouble();

            // assert
            Assert.Equal(0, randomNumber);
        }
    }
}