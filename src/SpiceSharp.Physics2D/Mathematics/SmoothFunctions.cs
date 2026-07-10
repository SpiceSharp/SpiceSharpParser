using System;

namespace SpiceSharp.Physics2D.Mathematics
{
    /// <summary>
    /// Provides shared smooth regularizations and their analytic derivatives.
    /// </summary>
    public static class SmoothFunctions
    {
        /// <summary>
        /// Evaluates <c>0.5 * (x + sqrt(x^2 + epsilon^2))</c> using a stable form.
        /// </summary>
        public static double PositivePart(double x, double epsilon)
        {
            ValidatePositiveFinite(epsilon, nameof(epsilon));
            double root = Hypot(x, epsilon);
            if (x >= 0.0)
            {
                return x + (0.5 * (root - x));
            }

            double normalizedX = x / root;
            return ((0.5 * epsilon) * (epsilon / root)) / (1.0 - normalizedX);
        }

        /// <summary>
        /// Evaluates the derivative of <see cref="PositivePart(double, double)"/> with respect to x.
        /// </summary>
        public static double PositivePartDerivative(double x, double epsilon)
        {
            ValidatePositiveFinite(epsilon, nameof(epsilon));
            double root = Hypot(x, epsilon);
            if (x >= 0.0)
            {
                return 0.5 * (1.0 + (x / root));
            }

            double epsilonRatio = epsilon / root;
            return (0.5 * epsilonRatio * epsilonRatio) / (1.0 - (x / root));
        }

        /// <summary>
        /// Evaluates <c>0.5 * (x - sqrt(x^2 + epsilon^2))</c> using a stable form.
        /// </summary>
        public static double NegativePart(double x, double epsilon) => -PositivePart(-x, epsilon);

        /// <summary>
        /// Evaluates the derivative of <see cref="NegativePart(double, double)"/> with respect to x.
        /// </summary>
        public static double NegativePartDerivative(double x, double epsilon) =>
            PositivePartDerivative(-x, epsilon);

        /// <summary>
        /// Evaluates the smooth absolute value <c>sqrt(x^2 + epsilon^2)</c>.
        /// </summary>
        public static double Absolute(double x, double epsilon)
        {
            ValidatePositiveFinite(epsilon, nameof(epsilon));
            return Hypot(x, epsilon);
        }

        /// <summary>
        /// Evaluates the derivative of <see cref="Absolute(double, double)"/> with respect to x.
        /// </summary>
        public static double AbsoluteDerivative(double x, double epsilon)
        {
            ValidatePositiveFinite(epsilon, nameof(epsilon));
            return x / Hypot(x, epsilon);
        }

        /// <summary>
        /// Evaluates the dimensionless regularized friction factor
        /// <c>tanh(slipVelocity / smoothingSpeed)</c>.
        /// </summary>
        public static double TanhFriction(double slipVelocity, double smoothingSpeed)
        {
            ValidatePositiveFinite(smoothingSpeed, nameof(smoothingSpeed));
            return Math.Tanh(slipVelocity / smoothingSpeed);
        }

        /// <summary>
        /// Evaluates the derivative of <see cref="TanhFriction(double, double)"/>
        /// with respect to slip velocity.
        /// </summary>
        public static double TanhFrictionDerivative(double slipVelocity, double smoothingSpeed)
        {
            ValidatePositiveFinite(smoothingSpeed, nameof(smoothingSpeed));
            double value = Math.Tanh(slipVelocity / smoothingSpeed);
            return (1.0 - (value * value)) / smoothingSpeed;
        }

        /// <summary>
        /// Evaluates <c>sqrt(vector.X^2 + vector.Y^2 + epsilon^2)</c> without
        /// avoidable intermediate overflow.
        /// </summary>
        public static double RegularizedLength(Vector2D vector, double epsilon)
        {
            ValidatePositiveFinite(epsilon, nameof(epsilon));
            return Hypot(vector.X, vector.Y, epsilon);
        }

        /// <summary>
        /// Evaluates the gradient of <see cref="RegularizedLength(Vector2D, double)"/>
        /// with respect to the vector coordinates.
        /// </summary>
        public static Vector2D RegularizedLengthGradient(Vector2D vector, double epsilon)
        {
            double length = RegularizedLength(vector, epsilon);
            return vector / length;
        }

        private static double Hypot(double x, double y)
        {
            double scale = Math.Max(Math.Abs(x), Math.Abs(y));
            if (scale == 0.0)
            {
                return 0.0;
            }

            double scaledX = x / scale;
            double scaledY = y / scale;
            return scale * Math.Sqrt((scaledX * scaledX) + (scaledY * scaledY));
        }

        private static double Hypot(double x, double y, double z)
        {
            double scale = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
            double scaledX = x / scale;
            double scaledY = y / scale;
            double scaledZ = z / scale;
            return scale * Math.Sqrt(
                (scaledX * scaledX) + (scaledY * scaledY) + (scaledZ * scaledZ));
        }

        private static void ValidatePositiveFinite(double value, string parameterName)
        {
            if (!(value > 0.0) || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be finite and greater than zero.");
            }
        }
    }
}
