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
    /// the parallel/series diode multipliers.
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
        /// <param name="current">The resulting local diode current.</param>
        /// <param name="conductance">The resulting small-signal conductance.</param>
        public static void Evaluate(
            IdealDiodeParameters parameters,
            BiasingParameters biasingParameters,
            double voltage,
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
                // Reverse breakdown is anchored to the off-line current at
                // -Vrev. With finite Roff, LTspice keeps the nominal knee at
                // -Vrev rather than moving it to the mathematical line
                // intersection.
                // If Rrev is omitted, LTspice-style behavior falls back to Ron.
                double reverseVoltage = Math.Abs(parameters.ReverseVoltage.Value);
                double reverseResistance = parameters.ReverseResistance.Given
                    ? parameters.ReverseResistance.Value
                    : parameters.OnResistance;
                double reverseConductance = 1.0 / reverseResistance;
                double reverseBoundary = -reverseVoltage;
                double reverseIntercept = (reverseConductance - offConductance) * reverseVoltage;

                // Evaluate the reverse-to-off knee. With revepsilon omitted or
                // zero this is a hard switch at -Vrev; otherwise the smoothing
                // window extends from the nominal boundary into reverse breakdown.
                EvaluateTransition(
                    voltage,
                    reverseBoundary,
                    parameters.ReverseEpsilon,
                    reverseConductance,
                    reverseIntercept,
                    offConductance,
                    0.0,
                    false,
                    out current,
                    out conductance);
            }

            // The forward on-line is anchored to the off-line current at Vfwd.
            // With finite Roff, LTspice keeps the nominal knee at Vfwd rather
            // than moving it to the mathematical line intersection.
            double onIntercept = (offConductance - onConductance) * forwardVoltage;
            double forwardBoundary = forwardVoltage;

            // Avoid a second transition evaluation in the off or reverse regions.
            // LTspice-style forward epsilon starts at the boundary and extends
            // into forward conduction, so voltages below the boundary remain off.
            if (voltage >= forwardBoundary)
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
                    true,
                    out current,
                    out conductance);
            }

            // Current limits are applied to the already-selected region. This keeps
            // the normal piecewise law simple and lets the limiter scale the local
            // derivative by the tanh derivative.
            ApplyCurrentLimits(parameters, ref current, ref conductance);

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
        /// <param name="smoothTowardRight">
        /// If <c>true</c>, the smoothing window starts at <paramref name="boundary" /> and extends toward
        /// larger voltages. If <c>false</c>, the window ends at <paramref name="boundary" /> and extends
        /// toward smaller voltages.
        /// </param>
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
            bool smoothTowardRight,
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

            // LTspice uses epsilon as a one-sided width, not as a centered band.
            // Forward smoothing starts at Vfwd and moves right into the on-region;
            // reverse smoothing starts in the reverse region and ends at -Vrev.
            double start = smoothTowardRight ? boundary : boundary - width;
            double end = smoothTowardRight ? boundary + width : boundary;
            double slopeDelta = rightSlope - leftSlope;

            // A one-sided conductance ramp shifts the fully conducting side by
            // half the slope change times the epsilon width. This keeps current
            // continuous where the ramp meets the straight-line region.
            double lowVoltageIntercept = smoothTowardRight
                ? leftIntercept
                : leftIntercept - (slopeDelta * width / 2.0);
            double highVoltageIntercept = smoothTowardRight
                ? rightIntercept - (slopeDelta * width / 2.0)
                : rightIntercept;

            if (voltage <= start)
            {
                current = (leftSlope * voltage) + lowVoltageIntercept;
                conductance = leftSlope;
                return;
            }

            if (voltage >= end)
            {
                current = (rightSlope * voltage) + highVoltageIntercept;
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
            current = (leftSlope * start) + lowVoltageIntercept
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
