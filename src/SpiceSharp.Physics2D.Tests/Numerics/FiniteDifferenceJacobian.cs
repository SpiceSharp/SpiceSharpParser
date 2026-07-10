using System;
using System.Collections.Generic;

namespace SpiceSharp.Physics2D.Tests.Numerics
{
    internal static class FiniteDifferenceJacobian
    {
        public static double CentralDerivative(Func<double, double> function, double point, double step)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            ValidatePositiveFinite(step, nameof(step));
            return (function(point + step) - function(point - step)) / (2.0 * step);
        }

        public static double[,] Calculate(
            Func<double[], double[]> function,
            IReadOnlyList<double> point,
            double relativeStep = 1e-5,
            double minimumStep = 1e-7)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            if (point == null)
            {
                throw new ArgumentNullException(nameof(point));
            }

            ValidatePositiveFinite(relativeStep, nameof(relativeStep));
            ValidatePositiveFinite(minimumStep, nameof(minimumStep));

            var center = new double[point.Count];
            for (int column = 0; column < center.Length; column++)
            {
                center[column] = point[column];
            }

            double[] centerValue = function((double[])center.Clone());
            if (centerValue == null)
            {
                throw new InvalidOperationException("The evaluated function returned null.");
            }

            var jacobian = new double[centerValue.Length, center.Length];
            for (int column = 0; column < center.Length; column++)
            {
                double step = Math.Max(
                    minimumStep,
                    relativeStep * Math.Max(1.0, Math.Abs(center[column])));
                var plus = (double[])center.Clone();
                var minus = (double[])center.Clone();
                plus[column] += step;
                minus[column] -= step;

                double[] plusValue = function(plus);
                double[] minusValue = function(minus);
                if (plusValue == null || minusValue == null)
                {
                    throw new InvalidOperationException("The evaluated function returned null.");
                }

                if (plusValue.Length != centerValue.Length || minusValue.Length != centerValue.Length)
                {
                    throw new InvalidOperationException(
                        "The evaluated function changed its output dimension.");
                }

                for (int row = 0; row < centerValue.Length; row++)
                {
                    jacobian[row, column] =
                        (plusValue[row] - minusValue[row]) / (2.0 * step);
                }
            }

            return jacobian;
        }

        private static void ValidatePositiveFinite(double value, string parameterName)
        {
            if (!(value > 0.0) || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
