using SpiceSharp;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// LTspice-style idealized diode current law.
    /// </summary>
    internal static class IdealDiodeEquation
    {
        private const double MinimumConductance = 0.0;

        /// <summary>
        /// Evaluates the current and small-signal conductance for one ideal diode.
        /// </summary>
        /// <param name="parameters">The ideal diode parameters.</param>
        /// <param name="biasingParameters">The simulation biasing parameters.</param>
        /// <param name="voltage">The voltage across one diode.</param>
        /// <param name="area">The diode area multiplier.</param>
        /// <param name="current">The output current.</param>
        /// <param name="conductance">The output conductance.</param>
        public static void Evaluate(
            IdealDiodeParameters parameters,
            BiasingParameters biasingParameters,
            double voltage,
            double area,
            out double current,
            out double conductance)
        {
            double onConductance = 1.0 / parameters.OnResistance;

            double offConductance = parameters.OffResistance.Given
                ? 1.0 / parameters.OffResistance.Value
                : Math.Max(biasingParameters.Gmin, MinimumConductance);

            double forwardVoltage = parameters.ForwardVoltage.Given ? parameters.ForwardVoltage.Value : 0.0;

            current = offConductance * voltage;
            conductance = offConductance;

            if (parameters.ReverseVoltage.Given)
            {
                double reverseVoltage = Math.Abs(parameters.ReverseVoltage.Value);
                double reverseResistance = parameters.ReverseResistance.Given
                    ? parameters.ReverseResistance.Value
                    : parameters.OnResistance;
                double reverseConductance = 1.0 / reverseResistance;
                double reverseIntercept = reverseConductance * reverseVoltage;
                double boundary = FindIntersection(
                    reverseConductance,
                    reverseIntercept,
                    offConductance,
                    0.0,
                    -reverseVoltage);

                EvaluateTransition(
                    voltage,
                    boundary,
                    parameters.ReverseEpsilon,
                    reverseConductance,
                    reverseIntercept,
                    offConductance,
                    0.0,
                    out current,
                    out conductance);
            }

            double onIntercept = -onConductance * forwardVoltage;
            double forwardBoundary = FindIntersection(
                offConductance,
                0.0,
                onConductance,
                onIntercept,
                forwardVoltage);

            double forwardWidth = parameters.ForwardEpsilon.Given ? parameters.ForwardEpsilon.Value : 0.0;
            double forwardStart = forwardBoundary - (Math.Max(forwardWidth, 0.0) / 2.0);
            if (voltage > forwardBoundary || (forwardWidth > 0.0 && voltage >= forwardStart))
            {
                EvaluateTransition(
                    voltage,
                    forwardBoundary,
                    parameters.ForwardEpsilon,
                    offConductance,
                    0.0,
                    onConductance,
                    onIntercept,
                    out current,
                    out conductance);
            }

            ApplyCurrentLimits(parameters, ref current, ref conductance);

            current *= area;
            conductance *= area;
        }

        private static double FindIntersection(
            double leftSlope,
            double leftIntercept,
            double rightSlope,
            double rightIntercept,
            double fallback)
        {
            double denominator = leftSlope - rightSlope;
            if (Math.Abs(denominator) <= 1e-30)
                return fallback;
            return (rightIntercept - leftIntercept) / denominator;
        }

        private static void EvaluateTransition(
            double voltage,
            double boundary,
            GivenParameter<double> epsilon,
            double leftSlope,
            double leftIntercept,
            double rightSlope,
            double rightIntercept,
            out double current,
            out double conductance)
        {
            double width = epsilon.Given ? epsilon.Value : 0.0;
            if (width <= 0.0)
            {
                if (voltage < boundary)
                {
                    current = (leftSlope * voltage) + leftIntercept;
                    conductance = leftSlope;
                }
                else
                {
                    current = (rightSlope * voltage) + rightIntercept;
                    conductance = rightSlope;
                }

                return;
            }

            double start = boundary - (width / 2.0);
            double end = boundary + (width / 2.0);
            if (voltage <= start)
            {
                current = (leftSlope * voltage) + leftIntercept;
                conductance = leftSlope;
                return;
            }

            if (voltage >= end)
            {
                current = (rightSlope * voltage) + rightIntercept;
                conductance = rightSlope;
                return;
            }

            double distance = voltage - start;
            double slopeDelta = rightSlope - leftSlope;
            current = (leftSlope * start) + leftIntercept
                + (leftSlope * distance)
                + (slopeDelta * distance * distance / (2.0 * width));
            conductance = leftSlope + (slopeDelta * distance / width);
        }

        private static void ApplyCurrentLimits(IdealDiodeParameters parameters, ref double current, ref double conductance)
        {
            if (current > 0.0 && parameters.ForwardCurrentLimit.Given)
            {
                ApplyCurrentLimit(parameters.ForwardCurrentLimit.Value, ref current, ref conductance);
            }
            else if (current < 0.0 && parameters.ReverseCurrentLimit.Given)
            {
                ApplyCurrentLimit(parameters.ReverseCurrentLimit.Value, ref current, ref conductance);
            }
        }

        private static void ApplyCurrentLimit(double limit, ref double current, ref double conductance)
        {
            limit = Math.Abs(limit);
            if (limit <= 0.0)
                return;

            double normalized = current / limit;
            double limited = Math.Tanh(normalized);
            current = limit * limited;
            conductance *= 1.0 - (limited * limited);
        }
    }
}
