using SpiceSharpParser.Common.Mathematics.Probability;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Mathematics.Probability
{
    public class CurveTests
    {
        [Fact]
        public void When_Add_EmptyCurve_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(0, 0.5));

            // assert
            Assert.Equal(0.5, curve.GetFirstPoint().Y);
            Assert.Equal(0, curve.GetFirstPoint().X);
            Assert.Equal(0.5, curve.GetLastPoint().Y);
            Assert.Equal(0, curve.GetLastPoint().X);
        }

        [Fact]
        public void When_Add_Same_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(0, 0.5));
            curve.Add(new Point(0, 0.1));

            // assert
            Assert.Equal(0.5, curve.GetFirstPoint().Y);
            Assert.Equal(0, curve.GetFirstPoint().X);
            Assert.Equal(0.1, curve.GetLastPoint().Y);
            Assert.Equal(0, curve.GetLastPoint().X);
        }

        [Fact]
        public void When_Add_Count_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(0, 0.5));
            curve.Add(new Point(0, 0.1));
            curve.Add(new Point(1, 0.2));
            curve.Add(new Point(3, 0.3));

            // assert
            Assert.Equal(4, curve.PointsCount);
        }

        [Fact]
        public void When_Add_Mixed_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(3, 0.3));
            curve.Add(new Point(0, 0.5));
            curve.Add(new Point(1, 0.2));
            curve.Add(new Point(0, 0.1));

            // assert
            Assert.Equal(0.5, curve.GetFirstPoint().Y);
            Assert.Equal(0, curve.GetFirstPoint().X);
            Assert.Equal(0.3, curve.GetLastPoint().Y);
            Assert.Equal(3, curve.GetLastPoint().X);
        }

        [Fact]
        public void When_Clear_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(3, 0.3));
            curve.Add(new Point(0, 0.5));
            curve.Add(new Point(1, 0.2));
            curve.Add(new Point(0, 0.1));

            curve.Clear();

            // assert
            Assert.Equal(0, curve.PointsCount);
        }

        [Fact]
        public void When_ComputeAreaUnderCurve_Square_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(0, 1.0));
            curve.Add(new Point(1, 1.0));

            // assert
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve());
        }

        [Fact]
        public void When_ComputeAreaUnderCurve_Triangle_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(-1, 0));
            curve.Add(new Point(0, 1.0));
            curve.Add(new Point(1, 0));

            // assert
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve());
        }

        [Fact]
        public void When_ComputeAreaUnderCurve_Complex_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(-1, 0));
            curve.Add(new Point(0, 1.0));
            curve.Add(new Point(1, 0));
            curve.Add(new Point(2, 0));
            curve.Add(new Point(2, 2));
            curve.Add(new Point(5, 2));

            // assert
            Assert.Equal(7.0, curve.ComputeAreaUnderCurve());
        }

        [Fact]
        public void When_ComputeAreaUnderCurve_Limit_Complex_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(-1, 0));
            curve.Add(new Point(0, 1.0));
            curve.Add(new Point(1, 0));
            curve.Add(new Point(2, 0));
            curve.Add(new Point(2, 2));
            curve.Add(new Point(5, 2));

            // assert
            Assert.Equal(0.0, curve.ComputeAreaUnderCurve(1));
            Assert.Equal(0.5, curve.ComputeAreaUnderCurve(2));
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve(3));
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve(4));
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve(5));
            Assert.Equal(7.0, curve.ComputeAreaUnderCurve(6));
        }

        [Fact]
        public void When_ComputeAreaUnderCurve_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(-1, 0));
            curve.Add(new Point(0, 1.0));
            curve.Add(new Point(1, 0));
            curve.Add(new Point(2, 0));
            curve.Add(new Point(2, 2));
            curve.Add(new Point(5, 2));

            curve.ScaleY(1.0 / 7.0);

            // assert
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve());
        }

        [Fact]
        public void When_ComputeAreaUnderCurveWithParameter_Expect_Reference()
        {
            // arrange
            var curve = new Curve();

            // act
            curve.Add(new Point(-1, 0));
            curve.Add(new Point(0, 1.0));
            curve.Add(new Point(1, 0));
            curve.Add(new Point(2, 0));
            curve.Add(new Point(2, 2));
            curve.Add(new Point(5, 2));

            // assert
            Assert.Equal(0, curve.ComputeAreaUnderCurve(-1.0));
            Assert.Equal(0.125, curve.ComputeAreaUnderCurve(-0.5));
            Assert.Equal(0.5, curve.ComputeAreaUnderCurve(0.0));
            Assert.Equal(0.875, curve.ComputeAreaUnderCurve(0.5));
            Assert.Equal(1.0, curve.ComputeAreaUnderCurve(1.0));
        }
    }
}