using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    internal sealed class RepeatingPwl : ParameterSet<IWaveformDescription>, IWaveformDescription
    {
        public RepeatingPwl(
            IEnumerable<Point> prefixPoints,
            IEnumerable<Point> repeatPoints,
            double repeatStartTime,
            double? repeatCount)
        {
            PrefixPoints = prefixPoints?.ToArray() ?? Array.Empty<Point>();
            RepeatPoints = repeatPoints?.ToArray() ?? throw new ArgumentNullException(nameof(repeatPoints));
            RepeatStartTime = repeatStartTime;
            RepeatCount = repeatCount;
        }

        public IReadOnlyList<Point> PrefixPoints { get; }

        public IReadOnlyList<Point> RepeatPoints { get; }

        public double RepeatStartTime { get; }

        public double? RepeatCount { get; }

        public IWaveform Create(IBindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new Instance(
                context.GetState<IIntegrationMethod>(),
                PrefixPoints,
                RepeatPoints,
                RepeatStartTime,
                RepeatCount);
        }

        private sealed class Instance : IWaveform
        {
            private readonly IIntegrationMethod _method;
            private readonly Point[] _prefixPoints;
            private readonly Point[] _repeatPoints;
            private readonly double _repeatStartTime;
            private readonly double? _repeatCount;
            private readonly double _period;

            public Instance(
                IIntegrationMethod method,
                IReadOnlyList<Point> prefixPoints,
                IReadOnlyList<Point> repeatPoints,
                double repeatStartTime,
                double? repeatCount)
            {
                _method = method ?? throw new ArgumentNullException(nameof(method));
                _prefixPoints = prefixPoints?.ToArray() ?? Array.Empty<Point>();
                _repeatPoints = repeatPoints?.ToArray() ?? throw new ArgumentNullException(nameof(repeatPoints));
                _repeatStartTime = repeatStartTime;
                _repeatCount = repeatCount;
                _period = _repeatPoints[_repeatPoints.Length - 1].Time;
                Value = _repeatPoints[0].Value;
            }

            public double Value { get; private set; }

            public void Probe()
            {
                double time = _method.Time;
                if (time < _repeatStartTime)
                {
                    Value = _prefixPoints.Length == 0
                        ? _repeatPoints[0].Value
                        : Interpolate(_prefixPoints, time);
                    return;
                }

                double elapsed = time - _repeatStartTime;
                if (_repeatCount.HasValue && elapsed >= _period * _repeatCount.Value)
                {
                    Value = _repeatPoints[_repeatPoints.Length - 1].Value;
                    return;
                }

                int cycleIndex = (int)Math.Floor(elapsed / _period);
                double localTime = elapsed - (cycleIndex * _period);
                if (localTime < 0)
                {
                    localTime = 0;
                }

                Value = InterpolateRepeat(localTime, cycleIndex);
            }

            public void Accept()
            {
            }

            private double InterpolateRepeat(double localTime, int cycleIndex)
            {
                Point first = _repeatPoints[0];
                Point last = _repeatPoints[_repeatPoints.Length - 1];

                if (localTime <= first.Time)
                {
                    if (first.Time > 0)
                    {
                        if (cycleIndex > 0)
                        {
                            return Interpolate(0.0, last.Value, first.Time, first.Value, localTime);
                        }

                        if (_prefixPoints.Length > 0)
                        {
                            double previousValue = _prefixPoints[_prefixPoints.Length - 1].Value;
                            return Interpolate(0.0, previousValue, first.Time, first.Value, localTime);
                        }
                    }

                    return first.Value;
                }

                return Interpolate(_repeatPoints, localTime);
            }

            private static double Interpolate(IReadOnlyList<Point> points, double time)
            {
                if (time <= points[0].Time)
                {
                    return points[0].Value;
                }

                for (var i = 1; i < points.Count; i++)
                {
                    if (time <= points[i].Time)
                    {
                        return Interpolate(
                            points[i - 1].Time,
                            points[i - 1].Value,
                            points[i].Time,
                            points[i].Value,
                            time);
                    }
                }

                return points[points.Count - 1].Value;
            }

            private static double Interpolate(double x0, double y0, double x1, double y1, double x)
            {
                if (x1.Equals(x0))
                {
                    return y1;
                }

                return y0 + ((y1 - y0) * (x - x0) / (x1 - x0));
            }
        }
    }
}
