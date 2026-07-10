using System;

namespace SpiceSharp.Physics2D.Mathematics
{
    /// <summary>
    /// Represents a double-precision vector in two-dimensional Cartesian space.
    /// </summary>
    public readonly struct Vector2D : IEquatable<Vector2D>
    {
        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector2D Zero = new Vector2D(0.0, 0.0);

        /// <summary>
        /// The unit vector along the x-axis.
        /// </summary>
        public static readonly Vector2D UnitX = new Vector2D(1.0, 0.0);

        /// <summary>
        /// The unit vector along the y-axis.
        /// </summary>
        public static readonly Vector2D UnitY = new Vector2D(0.0, 1.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2D"/> struct.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets the squared Euclidean length.
        /// </summary>
        public double LengthSquared => (X * X) + (Y * Y);

        /// <summary>
        /// Gets the Euclidean length.
        /// </summary>
        public double Length => Hypot(X, Y);

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        public static Vector2D operator +(Vector2D left, Vector2D right) =>
            new Vector2D(left.X + right.X, left.Y + right.Y);

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        public static Vector2D operator -(Vector2D left, Vector2D right) =>
            new Vector2D(left.X - right.X, left.Y - right.Y);

        /// <summary>
        /// Negates a vector.
        /// </summary>
        public static Vector2D operator -(Vector2D value) => new Vector2D(-value.X, -value.Y);

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        public static Vector2D operator *(Vector2D vector, double scalar) =>
            new Vector2D(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        public static Vector2D operator *(double scalar, Vector2D vector) => vector * scalar;

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        public static Vector2D operator /(Vector2D vector, double scalar) =>
            new Vector2D(vector.X / scalar, vector.Y / scalar);

        /// <summary>
        /// Tests exact structural equality.
        /// </summary>
        public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);

        /// <summary>
        /// Tests exact structural inequality.
        /// </summary>
        public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);

        /// <summary>
        /// Calculates the dot product.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>The scalar dot product.</returns>
        public static double Dot(Vector2D left, Vector2D right) =>
            (left.X * right.X) + (left.Y * right.Y);

        /// <summary>
        /// Calculates the signed scalar two-dimensional cross product.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>The z-component of the three-dimensional cross product.</returns>
        public static double Cross(Vector2D left, Vector2D right) =>
            (left.X * right.Y) - (left.Y * right.X);

        /// <summary>
        /// Returns the vector rotated counterclockwise by 90 degrees.
        /// </summary>
        /// <returns>The perpendicular vector <c>(-Y, X)</c>.</returns>
        public Vector2D Perpendicular() => new Vector2D(-Y, X);

        /// <summary>
        /// Rotates the vector counterclockwise by an angle.
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The rotated vector.</returns>
        public Vector2D Rotate(double angle)
        {
            double cosine = Math.Cos(angle);
            double sine = Math.Sin(angle);
            return new Vector2D(
                (cosine * X) - (sine * Y),
                (sine * X) + (cosine * Y));
        }

        /// <summary>
        /// Returns a unit vector in the same direction.
        /// </summary>
        /// <param name="epsilon">The largest length treated as degenerate.</param>
        /// <returns>The normalized vector.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="epsilon"/> is negative or non-finite.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the vector length is not greater than <paramref name="epsilon"/>.
        /// </exception>
        public Vector2D Normalized(double epsilon)
        {
            ValidateTolerance(epsilon, nameof(epsilon));

            double length = Length;
            if (!IsFinite(length))
            {
                throw new InvalidOperationException("Cannot normalize a vector with non-finite length.");
            }

            if (!(length > epsilon))
            {
                throw new InvalidOperationException(
                    $"Cannot normalize a vector of length {length:R} with epsilon {epsilon:R}.");
            }

            return this / length;
        }

        /// <summary>
        /// Attempts to return a unit vector in the same direction.
        /// </summary>
        /// <param name="epsilon">The largest length treated as degenerate.</param>
        /// <param name="normalized">The normalized vector, or zero for a degenerate vector.</param>
        /// <returns><c>true</c> when normalization succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="epsilon"/> is negative or non-finite.
        /// </exception>
        public bool TryNormalize(double epsilon, out Vector2D normalized)
        {
            ValidateTolerance(epsilon, nameof(epsilon));

            double length = Length;
            if (!IsFinite(length))
            {
                throw new InvalidOperationException("Cannot normalize a vector with non-finite length.");
            }

            if (!(length > epsilon))
            {
                normalized = Zero;
                return false;
            }

            normalized = this / length;
            return true;
        }

        /// <summary>
        /// Compares vectors with scale-aware absolute and relative tolerances.
        /// </summary>
        /// <param name="other">The vector to compare.</param>
        /// <param name="absoluteTolerance">The absolute tolerance.</param>
        /// <param name="relativeTolerance">The relative tolerance.</param>
        /// <returns><c>true</c> when both coordinates satisfy the tolerance.</returns>
        public bool ApproximatelyEquals(
            Vector2D other,
            double absoluteTolerance,
            double relativeTolerance)
        {
            ValidateTolerance(absoluteTolerance, nameof(absoluteTolerance));
            ValidateTolerance(relativeTolerance, nameof(relativeTolerance));

            return ApproximatelyEqual(X, other.X, absoluteTolerance, relativeTolerance)
                && ApproximatelyEqual(Y, other.Y, absoluteTolerance, relativeTolerance);
        }

        /// <inheritdoc/>
        public bool Equals(Vector2D other) => X.Equals(other.X) && Y.Equals(other.Y);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Vector2D other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"({X:R}, {Y:R})";

        private static bool ApproximatelyEqual(
            double left,
            double right,
            double absoluteTolerance,
            double relativeTolerance)
        {
            if (!IsFinite(left) || !IsFinite(right))
            {
                return false;
            }

            if (left.Equals(right))
            {
                return true;
            }

            double scale = Math.Max(Math.Abs(left), Math.Abs(right));
            return Math.Abs(left - right) <= absoluteTolerance + (relativeTolerance * scale);
        }

        private static double Hypot(double x, double y)
        {
            double scale = Math.Max(Math.Abs(x), Math.Abs(y));
            if (double.IsInfinity(scale))
            {
                return double.PositiveInfinity;
            }

            if (scale == 0.0)
            {
                return 0.0;
            }

            double scaledX = x / scale;
            double scaledY = y / scale;
            return scale * Math.Sqrt((scaledX * scaledX) + (scaledY * scaledY));
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        private static void ValidateTolerance(double value, string parameterName)
        {
            if (value < 0.0 || !IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Tolerance must be finite and nonnegative.");
            }
        }
    }
}
