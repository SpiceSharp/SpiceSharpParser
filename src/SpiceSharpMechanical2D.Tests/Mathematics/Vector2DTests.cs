using SpiceSharpMechanical2D.Mathematics;
using SpiceSharpMechanical2D.Tests.Numerics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Mathematics
{
    public class Vector2DTests
    {
        [Fact]
        public void ArithmeticSatisfiesVectorIdentities()
        {
            var a = new Vector2D(2.5, -4.0);
            var b = new Vector2D(-1.5, 3.0);

            Assert.Equal(new Vector2D(1.0, -1.0), a + b);
            Assert.Equal(a, (a + b) - b);
            Assert.Equal(Vector2D.Zero, a + (-a));
            Assert.Equal(a, 0.5 * (a * 2.0));
            Assert.Equal(a, (a * 3.0) / 3.0);
        }

        [Fact]
        public void DotAndPerpendicularSatisfyOrthogonalityIdentities()
        {
            var vector = new Vector2D(3.0, 4.0);
            Vector2D perpendicular = vector.Perpendicular();

            Assert.Equal(new Vector2D(-4.0, 3.0), perpendicular);
            Assert.Equal(0.0, Vector2D.Dot(vector, perpendicular));
            Assert.Equal(vector.LengthSquared, Vector2D.Dot(vector, vector));
        }

        [Fact]
        public void CrossProductUsesRightHandedSignConvention()
        {
            Assert.Equal(1.0, Vector2D.Cross(Vector2D.UnitX, Vector2D.UnitY));
            Assert.Equal(-1.0, Vector2D.Cross(Vector2D.UnitY, Vector2D.UnitX));
            Assert.Equal(0.0, Vector2D.Cross(Vector2D.UnitX, Vector2D.UnitX));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.25)]
        [InlineData(-2.75)]
        [InlineData(31.125)]
        public void RotationPreservesLength(double angle)
        {
            var vector = new Vector2D(3.25, -7.5);
            Vector2D rotated = vector.Rotate(angle);

            NumericAssert.Equal(vector.Length, rotated.Length, 1e-14, 1e-14);
            Assert.True(rotated.ApproximatelyEquals(
                Matrix2x2D.CreateRotation(angle) * vector,
                1e-14,
                1e-14));
        }

        [Fact]
        public void NormalizationProducesUnitVectorWithoutChangingDirection()
        {
            var vector = new Vector2D(3.0, 4.0);
            Vector2D normalized = vector.Normalized(1e-12);

            NumericAssert.Equal(1.0, normalized.Length, 1e-15, 1e-15);
            NumericAssert.Equal(0.0, Vector2D.Cross(vector, normalized), 1e-15, 1e-15);
            Assert.True(vector.TryNormalize(1e-12, out Vector2D tried));
            Assert.Equal(normalized, tried);
        }

        [Fact]
        public void NormalizationHandlesDegenerateLengthExplicitly()
        {
            Assert.False(Vector2D.Zero.TryNormalize(1e-12, out Vector2D normalized));
            Assert.Equal(Vector2D.Zero, normalized);
            Assert.Throws<InvalidOperationException>(() => Vector2D.Zero.Normalized(1e-12));
            Assert.Throws<ArgumentOutOfRangeException>(() => Vector2D.UnitX.Normalized(-1.0));
            Assert.Throws<InvalidOperationException>(() =>
                new Vector2D(double.PositiveInfinity, 0.0).TryNormalize(1e-12, out _));
        }

        [Fact]
        public void ExactAndApproximateEqualityAreSeparate()
        {
            var exact = new Vector2D(1.0, -2.0);
            var nearby = new Vector2D(1.0 + 1e-12, -2.0 - 1e-12);

            Assert.NotEqual(exact, nearby);
            Assert.True(exact != nearby);
            Assert.True(exact.ApproximatelyEquals(nearby, 1e-11, 1e-11));
            Assert.False(exact.ApproximatelyEquals(nearby, 1e-14, 1e-14));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ApproximateEqualityRejectsMatchingNonfiniteComponents(double value)
        {
            var left = new Vector2D(value, 0.0);
            var right = new Vector2D(value, 0.0);

            Assert.False(left.ApproximatelyEquals(right, 0.0, 0.0));
        }

        [Fact]
        public void LengthAvoidsIntermediateOverflow()
        {
            var vector = new Vector2D(3e200, 4e200);

            Assert.True(double.IsFinite(vector.Length));
            NumericAssert.Equal(5e200, vector.Length, 0.0, 1e-15);
        }

        [Fact]
        public void AuthoritativeTypesContainNoFloatStateOrVector2Dependency()
        {
            Type[] types = { typeof(Vector2D), typeof(Matrix2x2D) };
            foreach (Type type in types)
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Assert.NotEmpty(fields);
                Assert.All(fields, field => Assert.Equal(typeof(double), field.FieldType));
            }

            string sourceRoot = FindSourceRoot();
            string mathematicsRoot = Path.Combine(sourceRoot, "SpiceSharpMechanical2D", "Mathematics");
            string source = string.Join(
                Environment.NewLine,
                Directory.EnumerateFiles(mathematicsRoot, "*.cs").Select(File.ReadAllText));

            Assert.DoesNotContain("System.Numerics.Vector2", source, StringComparison.Ordinal);
            Assert.DoesNotContain("float ", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Single", source, StringComparison.Ordinal);
        }

        private static string FindSourceRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "SpiceSharp-Parser.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Could not locate the repository source directory.");
        }
    }
}
