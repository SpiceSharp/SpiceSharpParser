using System;

namespace SpiceSharp.Physics2D.Mathematics
{
    /// <summary>
    /// Provides angle helpers for display and relative-angle calculations.
    /// </summary>
    public static class AngleMath
    {
        /// <summary>
        /// Two times pi.
        /// </summary>
        public const double TwoPi = 2.0 * Math.PI;

        /// <summary>
        /// Wraps an angle to the half-open interval [-pi, pi).
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The wrapped angle, or NaN for a non-finite input.</returns>
        public static double WrapSigned(double angle)
        {
            double wrapped = angle % TwoPi;
            if (wrapped < -Math.PI)
            {
                wrapped += TwoPi;
            }
            else if (wrapped >= Math.PI)
            {
                wrapped -= TwoPi;
            }

            return wrapped;
        }

        /// <summary>
        /// Wraps an angle to the half-open interval [0, 2*pi).
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The wrapped angle, or NaN for a non-finite input.</returns>
        public static double WrapPositive(double angle)
        {
            double wrapped = angle % TwoPi;
            if (wrapped < 0.0)
            {
                wrapped += TwoPi;
            }

            return wrapped;
        }

        /// <summary>
        /// Calculates the signed shortest rotation from one angle to another.
        /// </summary>
        /// <param name="from">The starting angle in radians.</param>
        /// <param name="to">The destination angle in radians.</param>
        /// <returns>The signed difference in [-pi, pi).</returns>
        public static double ShortestDifference(double from, double to) => WrapSigned(to - from);
    }
}
