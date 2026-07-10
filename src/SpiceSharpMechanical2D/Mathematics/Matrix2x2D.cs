using System;

namespace SpiceSharpMechanical2D.Mathematics
{
    /// <summary>
    /// Represents a double-precision two-by-two matrix.
    /// </summary>
    public readonly struct Matrix2x2D : IEquatable<Matrix2x2D>
    {
        /// <summary>
        /// The zero matrix.
        /// </summary>
        public static readonly Matrix2x2D Zero = new Matrix2x2D(0.0, 0.0, 0.0, 0.0);

        /// <summary>
        /// The identity matrix.
        /// </summary>
        public static readonly Matrix2x2D Identity = new Matrix2x2D(1.0, 0.0, 0.0, 1.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix2x2D"/> struct.
        /// </summary>
        public Matrix2x2D(double m11, double m12, double m21, double m22)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
        }

        /// <summary>Gets the row-one, column-one element.</summary>
        public double M11 { get; }

        /// <summary>Gets the row-one, column-two element.</summary>
        public double M12 { get; }

        /// <summary>Gets the row-two, column-one element.</summary>
        public double M21 { get; }

        /// <summary>Gets the row-two, column-two element.</summary>
        public double M22 { get; }

        /// <summary>Gets the determinant.</summary>
        public double Determinant => (M11 * M22) - (M12 * M21);

        /// <summary>Gets the transpose.</summary>
        public Matrix2x2D Transpose => new Matrix2x2D(M11, M21, M12, M22);

        /// <summary>Adds two matrices.</summary>
        public static Matrix2x2D operator +(Matrix2x2D left, Matrix2x2D right) =>
            new Matrix2x2D(
                left.M11 + right.M11,
                left.M12 + right.M12,
                left.M21 + right.M21,
                left.M22 + right.M22);

        /// <summary>Subtracts two matrices.</summary>
        public static Matrix2x2D operator -(Matrix2x2D left, Matrix2x2D right) =>
            new Matrix2x2D(
                left.M11 - right.M11,
                left.M12 - right.M12,
                left.M21 - right.M21,
                left.M22 - right.M22);

        /// <summary>Negates a matrix.</summary>
        public static Matrix2x2D operator -(Matrix2x2D value) =>
            new Matrix2x2D(-value.M11, -value.M12, -value.M21, -value.M22);

        /// <summary>Multiplies a matrix by a scalar.</summary>
        public static Matrix2x2D operator *(Matrix2x2D matrix, double scalar) =>
            new Matrix2x2D(
                matrix.M11 * scalar,
                matrix.M12 * scalar,
                matrix.M21 * scalar,
                matrix.M22 * scalar);

        /// <summary>Multiplies a matrix by a scalar.</summary>
        public static Matrix2x2D operator *(double scalar, Matrix2x2D matrix) => matrix * scalar;

        /// <summary>Divides a matrix by a scalar.</summary>
        public static Matrix2x2D operator /(Matrix2x2D matrix, double scalar) =>
            matrix * (1.0 / scalar);

        /// <summary>Multiplies two matrices.</summary>
        public static Matrix2x2D operator *(Matrix2x2D left, Matrix2x2D right) =>
            new Matrix2x2D(
                (left.M11 * right.M11) + (left.M12 * right.M21),
                (left.M11 * right.M12) + (left.M12 * right.M22),
                (left.M21 * right.M11) + (left.M22 * right.M21),
                (left.M21 * right.M12) + (left.M22 * right.M22));

        /// <summary>Multiplies a matrix by a column vector.</summary>
        public static Vector2D operator *(Matrix2x2D matrix, Vector2D vector) =>
            new Vector2D(
                (matrix.M11 * vector.X) + (matrix.M12 * vector.Y),
                (matrix.M21 * vector.X) + (matrix.M22 * vector.Y));

        /// <summary>Tests exact structural equality.</summary>
        public static bool operator ==(Matrix2x2D left, Matrix2x2D right) => left.Equals(right);

        /// <summary>Tests exact structural inequality.</summary>
        public static bool operator !=(Matrix2x2D left, Matrix2x2D right) => !left.Equals(right);

        /// <summary>
        /// Creates a counterclockwise rotation matrix.
        /// </summary>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix2x2D CreateRotation(double angle)
        {
            double cosine = Math.Cos(angle);
            double sine = Math.Sin(angle);
            return new Matrix2x2D(cosine, -sine, sine, cosine);
        }

        /// <summary>
        /// Compares matrices with scale-aware absolute and relative tolerances.
        /// </summary>
        public bool ApproximatelyEquals(
            Matrix2x2D other,
            double absoluteTolerance,
            double relativeTolerance) =>
            new Vector2D(M11, M12).ApproximatelyEquals(
                new Vector2D(other.M11, other.M12),
                absoluteTolerance,
                relativeTolerance)
            && new Vector2D(M21, M22).ApproximatelyEquals(
                new Vector2D(other.M21, other.M22),
                absoluteTolerance,
                relativeTolerance);

        /// <inheritdoc/>
        public bool Equals(Matrix2x2D other) =>
            M11.Equals(other.M11)
            && M12.Equals(other.M12)
            && M21.Equals(other.M21)
            && M22.Equals(other.M22);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Matrix2x2D other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = M11.GetHashCode();
                hash = (hash * 397) ^ M12.GetHashCode();
                hash = (hash * 397) ^ M21.GetHashCode();
                return (hash * 397) ^ M22.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"[{M11:R}, {M12:R}; {M21:R}, {M22:R}]";
    }
}
