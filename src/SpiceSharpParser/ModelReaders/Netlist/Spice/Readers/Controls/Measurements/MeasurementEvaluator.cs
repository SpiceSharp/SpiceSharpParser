using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements
{
    /// <summary>
    /// Collects simulation data points and computes measurement results.
    /// One instance is created per simulation per measurement, so there are no thread-safety concerns.
    /// </summary>
    public class MeasurementEvaluator
    {
        private readonly List<(double X, double Y)> _data = new List<(double X, double Y)>();
        private readonly List<(double X, double Y)> _findData = new List<(double X, double Y)>();

        /// <summary>
        /// Gets the measurement definition being evaluated.
        /// </summary>
        public MeasurementDefinition Definition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementEvaluator"/> class.
        /// </summary>
        /// <param name="definition">The measurement definition.</param>
        public MeasurementEvaluator(MeasurementDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// Collects a data point for the primary signal.
        /// </summary>
        /// <param name="x">The independent variable (time, frequency, sweep value).</param>
        /// <param name="y">The signal value at x.</param>
        public void CollectDataPoint(double x, double y)
        {
            _data.Add((x, y));
        }

        /// <summary>
        /// Collects a data point for the FIND signal (used in FIND/WHEN measurements).
        /// </summary>
        /// <param name="x">The independent variable.</param>
        /// <param name="y">The FIND signal value at x.</param>
        public void CollectFindDataPoint(double x, double y)
        {
            _findData.Add((x, y));
        }

        /// <summary>
        /// Computes the measurement result from collected data.
        /// </summary>
        /// <param name="simulationName">The name of the simulation.</param>
        /// <returns>The computed measurement result.</returns>
        public MeasurementResult ComputeResult(string simulationName)
        {
            switch (Definition.Type)
            {
                case MeasType.TrigTarg:
                    // TrigTarg is handled inline in MeasControl — should not reach here
                    return new MeasurementResult(Definition.Name, double.NaN, false, "TRIG_TARG", simulationName);
                case MeasType.When:
                    return ComputeWhen(simulationName);
                case MeasType.FindWhen:
                    return ComputeFindWhen(simulationName);
                case MeasType.FindAt:
                    return ComputeFindAt(simulationName);
                case MeasType.Min:
                    return ComputeMin(simulationName);
                case MeasType.Max:
                    return ComputeMax(simulationName);
                case MeasType.Avg:
                    return ComputeAvg(simulationName);
                case MeasType.Rms:
                    return ComputeRms(simulationName);
                case MeasType.Pp:
                    return ComputePp(simulationName);
                case MeasType.Integ:
                    return ComputeInteg(simulationName);
                case MeasType.Deriv:
                    return ComputeDeriv(simulationName);
                default:
                    return new MeasurementResult(Definition.Name, double.NaN, false, Definition.Type.ToString().ToUpper(), simulationName);
            }
        }

        /// <summary>
        /// Finds the x-value at which the signal crosses the given threshold at the nth specified edge.
        /// </summary>
        internal static double? FindCrossing(
            List<(double X, double Y)> data,
            double threshold,
            EdgeType edgeType,
            int edgeNumber,
            double? td,
            double? fromX,
            double? toX)
        {
            bool findLast = edgeNumber == EdgeConstants.Last;
            int edgeCount = 0;
            double? lastCrossing = null;

            for (int i = 1; i < data.Count; i++)
            {
                double x0 = data[i - 1].X;
                double x1 = data[i].X;

                if (td.HasValue && x1 < td.Value)
                {
                    continue;
                }

                if (fromX.HasValue && x1 < fromX.Value)
                {
                    continue;
                }

                if (toX.HasValue && x0 > toX.Value)
                {
                    break;
                }

                double y0 = data[i - 1].Y - threshold;
                double y1 = data[i].Y - threshold;

                if (y0 == 0.0 && y1 == 0.0)
                {
                    continue;
                }

                bool crosses = y0 * y1 < 0 || (y0 == 0.0 && y1 != 0.0) || (y0 != 0.0 && y1 == 0.0);

                if (!crosses)
                {
                    continue;
                }

                bool isRising = y1 > y0;

                bool matchesEdge;
                switch (edgeType)
                {
                    case EdgeType.Rise:
                        matchesEdge = isRising;
                        break;
                    case EdgeType.Fall:
                        matchesEdge = !isRising;
                        break;
                    default:
                        matchesEdge = true;
                        break;
                }

                if (!matchesEdge)
                {
                    continue;
                }

                edgeCount++;

                // Linear interpolation to find precise crossing point
                double crossX;
                double dy = data[i].Y - data[i - 1].Y;
                if (Math.Abs(dy) < 1e-30)
                {
                    crossX = data[i].X;
                }
                else
                {
                    double fraction = (threshold - data[i - 1].Y) / dy;
                    crossX = data[i - 1].X + fraction * (data[i].X - data[i - 1].X);
                }

                if (findLast)
                {
                    lastCrossing = crossX;
                }
                else if (edgeCount == edgeNumber)
                {
                    return crossX;
                }
            }

            return findLast ? lastCrossing : null;
        }

        /// <summary>
        /// Interpolates the y-value at a given x from a data series using linear interpolation.
        /// </summary>
        internal static double InterpolateY(List<(double X, double Y)> data, double targetX)
        {
            if (data.Count == 0)
            {
                return double.NaN;
            }

            if (targetX <= data[0].X)
            {
                return data[0].Y;
            }

            if (targetX >= data[data.Count - 1].X)
            {
                return data[data.Count - 1].Y;
            }

            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].X >= targetX)
                {
                    double dx = data[i].X - data[i - 1].X;
                    if (Math.Abs(dx) < 1e-30)
                    {
                        return data[i].Y;
                    }

                    double fraction = (targetX - data[i - 1].X) / dx;
                    return data[i - 1].Y + fraction * (data[i].Y - data[i - 1].Y);
                }
            }

            return data[data.Count - 1].Y;
        }

        private List<(double X, double Y)> GetWindowedData(List<(double X, double Y)> data)
        {
            if (!Definition.From.HasValue && !Definition.To.HasValue)
            {
                return data;
            }

            var result = new List<(double X, double Y)>();
            foreach (var point in data)
            {
                if (Definition.From.HasValue && point.X < Definition.From.Value)
                {
                    continue;
                }

                if (Definition.To.HasValue && point.X > Definition.To.Value)
                {
                    break;
                }

                result.Add(point);
            }

            return result;
        }

        // Note: TrigTarg measurements are handled inline in MeasControl.SetupMeasurement()
        // because they may require separate data collection for trigger and target signals.

        private MeasurementResult ComputeWhen(string simulationName)
        {
            double? crossX = FindCrossing(_data, Definition.WhenVal, Definition.WhenEdge, Definition.WhenEdgeNumber, Definition.WhenTd, Definition.From, Definition.To);
            if (!crossX.HasValue)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "WHEN", simulationName);
            }

            return new MeasurementResult(Definition.Name, crossX.Value, true, "WHEN", simulationName);
        }

        private MeasurementResult ComputeFindWhen(string simulationName)
        {
            // Use the primary data (_data) for the WHEN condition
            double? crossX = FindCrossing(_data, Definition.WhenVal, Definition.WhenEdge, Definition.WhenEdgeNumber, Definition.WhenTd, Definition.From, Definition.To);
            if (!crossX.HasValue)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "FIND_WHEN", simulationName);
            }

            // Interpolate the FIND signal at the crossing x-value
            var findSource = _findData.Count > 0 ? _findData : _data;
            double value = InterpolateY(findSource, crossX.Value);
            return new MeasurementResult(Definition.Name, value, true, "FIND_WHEN", simulationName);
        }

        private MeasurementResult ComputeFindAt(string simulationName)
        {
            if (!Definition.At.HasValue)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "FIND_AT", simulationName);
            }

            if (_data.Count == 0)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "FIND_AT", simulationName);
            }

            double value = InterpolateY(_data, Definition.At.Value);
            return new MeasurementResult(Definition.Name, value, true, "FIND_AT", simulationName);
        }

        private MeasurementResult ComputeMin(string simulationName)
        {
            var windowed = GetWindowedData(_data);
            if (windowed.Count == 0)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "MIN", simulationName);
            }

            double min = double.MaxValue;
            foreach (var point in windowed)
            {
                if (point.Y < min)
                {
                    min = point.Y;
                }
            }

            return new MeasurementResult(Definition.Name, min, true, "MIN", simulationName);
        }

        private MeasurementResult ComputeMax(string simulationName)
        {
            var windowed = GetWindowedData(_data);
            if (windowed.Count == 0)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "MAX", simulationName);
            }

            double max = double.MinValue;
            foreach (var point in windowed)
            {
                if (point.Y > max)
                {
                    max = point.Y;
                }
            }

            return new MeasurementResult(Definition.Name, max, true, "MAX", simulationName);
        }

        private MeasurementResult ComputeAvg(string simulationName)
        {
            var windowed = GetWindowedData(_data);
            if (windowed.Count < 2)
            {
                if (windowed.Count == 1)
                {
                    return new MeasurementResult(Definition.Name, windowed[0].Y, true, "AVG", simulationName);
                }

                return new MeasurementResult(Definition.Name, double.NaN, false, "AVG", simulationName);
            }

            // Trapezoidal mean: integral(y dx) / (xEnd - xStart)
            double integral = 0;
            for (int i = 1; i < windowed.Count; i++)
            {
                double dx = windowed[i].X - windowed[i - 1].X;
                integral += (windowed[i].Y + windowed[i - 1].Y) * 0.5 * dx;
            }

            double span = windowed[windowed.Count - 1].X - windowed[0].X;
            if (Math.Abs(span) < 1e-30)
            {
                return new MeasurementResult(Definition.Name, windowed[0].Y, true, "AVG", simulationName);
            }

            return new MeasurementResult(Definition.Name, integral / span, true, "AVG", simulationName);
        }

        private MeasurementResult ComputeRms(string simulationName)
        {
            var windowed = GetWindowedData(_data);
            if (windowed.Count < 2)
            {
                if (windowed.Count == 1)
                {
                    return new MeasurementResult(Definition.Name, Math.Abs(windowed[0].Y), true, "RMS", simulationName);
                }

                return new MeasurementResult(Definition.Name, double.NaN, false, "RMS", simulationName);
            }

            // Trapezoidal RMS: sqrt(integral(y^2 dx) / (xEnd - xStart))
            double integral = 0;
            for (int i = 1; i < windowed.Count; i++)
            {
                double dx = windowed[i].X - windowed[i - 1].X;
                double y0Sq = windowed[i - 1].Y * windowed[i - 1].Y;
                double y1Sq = windowed[i].Y * windowed[i].Y;
                integral += (y0Sq + y1Sq) * 0.5 * dx;
            }

            double span = windowed[windowed.Count - 1].X - windowed[0].X;
            if (Math.Abs(span) < 1e-30)
            {
                return new MeasurementResult(Definition.Name, Math.Abs(windowed[0].Y), true, "RMS", simulationName);
            }

            return new MeasurementResult(Definition.Name, Math.Sqrt(Math.Max(0, integral / span)), true, "RMS", simulationName);
        }

        private MeasurementResult ComputePp(string simulationName)
        {
            var windowed = GetWindowedData(_data);
            if (windowed.Count == 0)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "PP", simulationName);
            }

            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (var point in windowed)
            {
                if (point.Y < min) min = point.Y;
                if (point.Y > max) max = point.Y;
            }

            return new MeasurementResult(Definition.Name, max - min, true, "PP", simulationName);
        }

        private MeasurementResult ComputeInteg(string simulationName)
        {
            var windowed = GetWindowedData(_data);
            if (windowed.Count < 2)
            {
                return new MeasurementResult(Definition.Name, windowed.Count == 1 ? 0 : double.NaN, windowed.Count == 1, "INTEG", simulationName);
            }

            double integral = 0;
            for (int i = 1; i < windowed.Count; i++)
            {
                double dx = windowed[i].X - windowed[i - 1].X;
                integral += (windowed[i].Y + windowed[i - 1].Y) * 0.5 * dx;
            }

            return new MeasurementResult(Definition.Name, integral, true, "INTEG", simulationName);
        }

        private MeasurementResult ComputeDeriv(string simulationName)
        {
            double targetX;

            if (Definition.At.HasValue)
            {
                targetX = Definition.At.Value;
            }
            else if (Definition.WhenSignal != null)
            {
                // DERIV ... WHEN: find the crossing point first using _data (the WHEN signal)
                double? crossX = FindCrossing(_data, Definition.WhenVal, Definition.WhenEdge, Definition.WhenEdgeNumber, Definition.WhenTd, Definition.From, Definition.To);
                if (!crossX.HasValue)
                {
                    return new MeasurementResult(Definition.Name, double.NaN, false, "DERIV", simulationName);
                }

                targetX = crossX.Value;
            }
            else
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "DERIV", simulationName);
            }

            // Use _findData for the derivative signal when WHEN is used, otherwise _data
            var derivData = _findData.Count > 0 ? _findData : _data;
            var windowed = GetWindowedData(derivData);

            if (windowed.Count < 2)
            {
                return new MeasurementResult(Definition.Name, double.NaN, false, "DERIV", simulationName);
            }

            // Find the interval containing targetX and compute derivative using
            // the slope of the interval (linear interpolation derivative).
            // For interior points where central difference is available, use it
            // for better accuracy.
            for (int i = 1; i < windowed.Count; i++)
            {
                if (windowed[i].X >= targetX)
                {
                    // Compute slope of the interval [i-1, i] containing targetX
                    double dxInterval = windowed[i].X - windowed[i - 1].X;
                    if (Math.Abs(dxInterval) < 1e-30)
                    {
                        return new MeasurementResult(Definition.Name, double.NaN, false, "DERIV", simulationName);
                    }

                    double intervalSlope = (windowed[i].Y - windowed[i - 1].Y) / dxInterval;

                    // If targetX is very close to point i and central difference is available, use it
                    double distToI = Math.Abs(windowed[i].X - targetX);
                    if (distToI < dxInterval * 0.01 && i > 0 && i < windowed.Count - 1)
                    {
                        double dxCentral = windowed[i + 1].X - windowed[i - 1].X;
                        if (Math.Abs(dxCentral) >= 1e-30)
                        {
                            double centralDeriv = (windowed[i + 1].Y - windowed[i - 1].Y) / dxCentral;
                            return new MeasurementResult(Definition.Name, centralDeriv, true, "DERIV", simulationName);
                        }
                    }

                    return new MeasurementResult(Definition.Name, intervalSlope, true, "DERIV", simulationName);
                }
            }

            // targetX is beyond data range — use slope of last interval
            if (windowed.Count >= 2)
            {
                int last = windowed.Count - 1;
                double dxLast = windowed[last].X - windowed[last - 1].X;
                if (Math.Abs(dxLast) >= 1e-30)
                {
                    double lastSlope = (windowed[last].Y - windowed[last - 1].Y) / dxLast;
                    return new MeasurementResult(Definition.Name, lastSlope, true, "DERIV", simulationName);
                }
            }

            return new MeasurementResult(Definition.Name, double.NaN, false, "DERIV", simulationName);
        }
    }
}
