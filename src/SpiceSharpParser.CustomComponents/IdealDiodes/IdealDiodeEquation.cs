using SpiceSharp;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// LTspice-style idealized diode current law.
    /// </summary>
    /// <remarks>
    /// This helper owns only the local current law for one ideal diode cell. The
    /// biasing behavior that calls it handles the external branch details such as
    /// series resistance and the parallel/series diode multipliers.
    ///
    /// The current law is built from straight-line regions in slope/intercept
    /// form: <c>I = g * V + b</c>. The off region uses <c>Roff</c> or simulator
    /// <c>Gmin</c>, the forward region uses <c>Ron</c> after <c>Vfwd</c>, and the
    /// optional reverse-breakdown region uses <c>Rrev</c> after <c>-Vrev</c>.
    /// Optional epsilon parameters replace abrupt knees with a finite-width
    /// conductance ramp so both current and conductance stay continuous for
    /// Newton iteration.
    /// </remarks>
    internal static class IdealDiodeEquation
    {
        /// <summary>
        /// Lowest fallback off-state conductance when neither <c>Roff</c> nor simulator <c>Gmin</c> supplies one.
        /// </summary>
        private const double MinimumConductance = 0.0;

        /// <summary>
        /// Evaluates the current and small-signal conductance for one ideal diode cell.
        /// </summary>
        /// <param name="parameters">The effective model and instance parameters.</param>
        /// <param name="biasingParameters">The simulation biasing parameters, used for the default off conductance.</param>
        /// <param name="voltage">The local voltage across one diode cell.</param>
        /// <param name="area">The diode area multiplier applied after the local equation is evaluated.</param>
        /// <param name="current">The resulting local diode current, scaled by <paramref name="area" />.</param>
        /// <param name="conductance">The resulting small-signal conductance, scaled by <paramref name="area" />.</param>
        public static void Evaluate(
            IdealDiodeParameters parameters,
            BiasingParameters biasingParameters,
            double voltage,
            double area,
            out double current,
            out double conductance)
        {
            // Work in conductance because the solver needs dI/dV. Each operating
            // region is represented as a line: current = slope * voltage + intercept.
            double onConductance = 1.0 / parameters.OnResistance;

            // LTspice's ideal diode can omit Roff. In that case the off-state
            // leakage follows the simulator's Gmin so the device still contributes
            // the same numerical shunt used elsewhere during biasing.
            double offConductance = parameters.OffResistance.Given
                ? 1.0 / parameters.OffResistance.Value
                : Math.Max(biasingParameters.Gmin, MinimumConductance);

            // Vfwd defaults to zero. With a zero threshold, the forward on-line is
            // simply Ron through the origin.
            double forwardVoltage = parameters.ForwardVoltage.Given ? parameters.ForwardVoltage.Value : 0.0;

            // Start from the off branch. Reverse breakdown and forward conduction
            // may overwrite this below when the voltage lies in their region.
            current = offConductance * voltage;
            conductance = offConductance;

            if (parameters.ReverseVoltage.Given)
            {
                // Reverse breakdown is a line through (-Vrev, 0):
                // current = Grev * voltage + Grev * Vrev.
                // If Rrev is omitted, LTspice-style behavior falls back to Ron.
                double reverseVoltage = Math.Abs(parameters.ReverseVoltage.Value);
                double reverseResistance = parameters.ReverseResistance.Given
                    ? parameters.ReverseResistance.Value
                    : parameters.OnResistance;
                double reverseConductance = 1.0 / reverseResistance;
                double reverseIntercept = reverseConductance * reverseVoltage;

                // Roff/Gmin can tilt the off-line, so the true intersection is not
                // always exactly -Vrev. Use the nominal knee only as a degenerate
                // fallback when the two lines are effectively parallel.
                double boundary = FindIntersection(
                    reverseConductance,
                    reverseIntercept,
                    offConductance,
                    0.0,
                    -reverseVoltage);

                // Evaluate the reverse-to-off knee. With revepsilon omitted or
                // zero this is a hard switch at the intersection; otherwise it is
                // smoothed symmetrically around the boundary.
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

            // The forward on-line crosses zero current at Vfwd:
            // current = Gon * voltage - Gon * Vfwd.
            double onIntercept = -onConductance * forwardVoltage;

            // Find the voltage where the off-line and the forward on-line meet.
            // This is close to Vfwd when Roff is large, but computing it keeps the
            // model continuous for any legal Roff/Gmin.
            double forwardBoundary = FindIntersection(
                offConductance,
                0.0,
                onConductance,
                onIntercept,
                forwardVoltage);

            // Avoid a second transition evaluation in the normal off region. For
            // smoothed knees, begin applying the blend at the start of the epsilon
            // band so the partial-conduction region is not missed.
            double forwardWidth = parameters.ForwardEpsilon.Given ? parameters.ForwardEpsilon.Value : 0.0;
            double forwardStart = forwardBoundary - (Math.Max(forwardWidth, 0.0) / 2.0);
            if (voltage > forwardBoundary || (forwardWidth > 0.0 && voltage >= forwardStart))
            {
                // Evaluate the off-to-forward knee using the same generic transition
                // helper used for reverse breakdown.
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

            // Current limits are applied to the already-selected region. This keeps
            // the normal piecewise law simple and lets the limiter scale the local
            // derivative by the tanh derivative.
            ApplyCurrentLimits(parameters, ref current, ref conductance);

            // Area behaves like parallel identical cells: both DC current and
            // small-signal conductance scale linearly.
            current *= area;
            conductance *= area;
        }

        /// <summary>
        /// Finds where two line segments in <c>current = slope * voltage + intercept</c> form intersect.
        /// </summary>
        /// <param name="leftSlope">The slope of the first line.</param>
        /// <param name="leftIntercept">The intercept of the first line.</param>
        /// <param name="rightSlope">The slope of the second line.</param>
        /// <param name="rightIntercept">The intercept of the second line.</param>
        /// <param name="fallback">The voltage to use if the lines are nearly parallel.</param>
        /// <returns>The intersection voltage.</returns>
        private static double FindIntersection(
            double leftSlope,
            double leftIntercept,
            double rightSlope,
            double rightIntercept,
            double fallback)
        {
            double denominator = leftSlope - rightSlope;

            // Parallel or nearly parallel regions do not give a useful numerical
            // knee. The caller provides the physically meaningful nominal boundary.
            if (Math.Abs(denominator) <= 1e-30)
                return fallback;

            return (rightIntercept - leftIntercept) / denominator;
        }

        /// <summary>
        /// Evaluates either a hard or smoothed transition between two linear current regions.
        /// </summary>
        /// <param name="voltage">The voltage at which to evaluate the transition.</param>
        /// <param name="boundary">The intersection voltage of the two unsmoothed lines.</param>
        /// <param name="epsilon">The optional smoothing width around <paramref name="boundary" />.</param>
        /// <param name="leftSlope">The slope used below the transition.</param>
        /// <param name="leftIntercept">The intercept used below the transition.</param>
        /// <param name="rightSlope">The slope used above the transition.</param>
        /// <param name="rightIntercept">The intercept used above the transition.</param>
        /// <param name="current">The evaluated current.</param>
        /// <param name="conductance">The evaluated conductance.</param>
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
                // No smoothing requested: choose the line on the active side of
                // the intersection and expose that line's slope as conductance.
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

            // The epsilon value is the full width of the blend, centered on the
            // natural intersection of the two lines.
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

            // Inside the smoothing band the conductance moves linearly from the
            // left slope to the right slope. Current is the integral of that ramp,
            // anchored to the left line at the start of the band:
            //
            // g(x) = leftSlope + slopeDelta * x / width
            // I(x) = I(start) + leftSlope * x + slopeDelta * x^2 / (2 * width)
            //
            // where x is the distance from the start of the smoothing band.
            double distance = voltage - start;
            double slopeDelta = rightSlope - leftSlope;
            current = (leftSlope * start) + leftIntercept
                + (leftSlope * distance)
                + (slopeDelta * distance * distance / (2.0 * width));
            conductance = leftSlope + (slopeDelta * distance / width);
        }

        /// <summary>
        /// Applies the optional forward or reverse current limiter to the selected current branch.
        /// </summary>
        /// <param name="parameters">The effective ideal diode parameters.</param>
        /// <param name="current">The current to limit in place.</param>
        /// <param name="conductance">The conductance to update in place with the limiter derivative.</param>
        private static void ApplyCurrentLimits(IdealDiodeParameters parameters, ref double current, ref double conductance)
        {
            // Forward and reverse limits are independent. Select by the sign of the
            // already-computed current so leakage, breakdown, and smoothed knees all
            // feed into the same limiting law.
            if (current > 0.0 && parameters.ForwardCurrentLimit.Given)
            {
                ApplyCurrentLimit(parameters.ForwardCurrentLimit.Value, ref current, ref conductance);
            }
            else if (current < 0.0 && parameters.ReverseCurrentLimit.Given)
            {
                ApplyCurrentLimit(parameters.ReverseCurrentLimit.Value, ref current, ref conductance);
            }
        }

        /// <summary>
        /// Smoothly compresses current toward a symmetric magnitude limit.
        /// </summary>
        /// <param name="limit">The positive or negative limit magnitude.</param>
        /// <param name="current">The current to limit in place.</param>
        /// <param name="conductance">The conductance to update in place with the limiter derivative.</param>
        private static void ApplyCurrentLimit(double limit, ref double current, ref double conductance)
        {
            limit = Math.Abs(limit);
            if (limit <= 0.0)
                return;

            // I_limited = limit * tanh(I_raw / limit)
            // dI_limited/dV = dI_raw/dV * (1 - tanh(I_raw / limit)^2)
            // This gives a soft saturation without a derivative discontinuity.
            double normalized = current / limit;
            double limited = Math.Tanh(normalized);
            current = limit * limited;
            conductance *= 1.0 - (limited * limited);
        }
    }
}
