using SpiceSharpParser.Common.Mathematics.Probability;
using System;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Mathematics.Probability
{
    public class PdfTests
    {
        [Fact]
        public void When_EmptyCurve_Expect_Exception()
        {
            // arrange, act, assert
            Assert.Throws<ArgumentException>(() => new Pdf(new Curve()));
        }

        [Fact]
        public void When_TriangleCurve_Expect_Reference()
        {
            // arrange
            var triangle = new Curve();
            triangle.Add(new Point(-1.0, 0));
            triangle.Add(new Point(0, 1));
            triangle.Add(new Point(1, 0));

            // act
            var pdfTriangle = new Pdf(triangle);

            // assert
            Assert.Equal(1.0, pdfTriangle.ComputeAreaUnderCurve());
        }

        [Fact]
        public void When_HigherTriangleCurve_Expect_Reference()
        {
            // arrange
            var triangle = new Curve();
            triangle.Add(new Point(-1.0, 0));
            triangle.Add(new Point(0, 3));
            triangle.Add(new Point(1, 0));

            // act
            var pdfTriangle = new Pdf(triangle);

            // assert
            Assert.Equal(1.0, pdfTriangle.ComputeAreaUnderCurve());
        }
    }
}