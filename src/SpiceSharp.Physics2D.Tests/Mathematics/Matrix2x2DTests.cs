using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Physics2D.Tests.Numerics;
using System;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Mathematics
{
    public class Matrix2x2DTests
    {
        [Fact]
        public void IdentityAndZeroSatisfyAlgebraicIdentities()
        {
            var matrix = new Matrix2x2D(1.0, 2.0, 3.0, 4.0);
            var vector = new Vector2D(-2.0, 5.0);

            Assert.Equal(matrix, Matrix2x2D.Identity * matrix);
            Assert.Equal(matrix, matrix * Matrix2x2D.Identity);
            Assert.Equal(vector, Matrix2x2D.Identity * vector);
            Assert.Equal(Matrix2x2D.Zero, matrix + (-matrix));
            Assert.Equal(matrix, (matrix * 4.0) / 4.0);
        }

        [Fact]
        public void MatrixMultiplicationAndDeterminantMatchKnownResult()
        {
            var left = new Matrix2x2D(1.0, 2.0, 3.0, 4.0);
            var right = new Matrix2x2D(5.0, 6.0, 7.0, 8.0);

            Assert.Equal(new Matrix2x2D(19.0, 22.0, 43.0, 50.0), left * right);
            Assert.Equal(-2.0, left.Determinant);
            Assert.Equal(new Matrix2x2D(1.0, 3.0, 2.0, 4.0), left.Transpose);
        }

        [Fact]
        public void RotationMatrixIsOrthogonalAndHasUnitDeterminant()
        {
            Matrix2x2D rotation = Matrix2x2D.CreateRotation(1.2345);
            Matrix2x2D product = rotation.Transpose * rotation;

            Assert.True(product.ApproximatelyEquals(Matrix2x2D.Identity, 1e-14, 1e-14));
            NumericAssert.Equal(1.0, rotation.Determinant, 1e-14, 1e-14);
        }

        [Fact]
        public void ExactAndApproximateMatrixEqualityAreSeparate()
        {
            var exact = new Matrix2x2D(1.0, 2.0, 3.0, 4.0);
            var nearby = new Matrix2x2D(1.0, 2.0 + 1e-12, 3.0, 4.0);

            Assert.NotEqual(exact, nearby);
            Assert.True(exact.ApproximatelyEquals(nearby, 1e-11, 1e-11));
            Assert.False(exact.ApproximatelyEquals(nearby, 1e-14, 1e-14));
        }

        [Fact]
        public void ApproximateEqualityRejectsMatchingNonfiniteComponents()
        {
            var left = new Matrix2x2D(double.NaN, 0.0, 0.0, 1.0);
            var right = new Matrix2x2D(double.NaN, 0.0, 0.0, 1.0);

            Assert.False(left.ApproximatelyEquals(right, 0.0, 0.0));
        }
    }
}
