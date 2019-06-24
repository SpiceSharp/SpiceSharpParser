using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class Curve : IEnumerable<Point>
    {
        private readonly List<Point> _points;

        public Curve()
        {
            _points = new List<Point>();
        }

        public int PointsCount => _points.Count;

        public Point this[int index] => _points[index];

        public void Add(Point point)
        {
            for (var i = 0; i < _points.Count; i++)
            {
                if (_points[i].X > point.X)
                {
                    _points.Insert(i, point);
                    return;
                }
            }

            _points.Add(point);
        }

        public double ComputeAreaUnderCurve()
        {
            return ComputeAreaUnderCurve(_points.Count);
        }

        public double ComputeAreaUnderCurve(int limitPointIndex)
        {
            if (limitPointIndex > _points.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(limitPointIndex));
            }
            double result = 0.0;

            for (var i = 1; i < limitPointIndex; i++)
            {
                result += (_points[i].X - _points[i - 1].X) * (_points[i].Y + _points[i - 1].Y) / 2.0;
            }

            return result;
        }

        public void ScaleY(double scaleFactor)
        {
            _points.ForEach(p => p.Y *= scaleFactor);
        }

        public Point GetFirstPoint()
        {
            return _points.FirstOrDefault();
        }

        public Point GetLastPoint()
        {
            return _points.LastOrDefault();
        }

        public void Clear()
        {
            _points.Clear();
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Curve Clone()
        {
            var cloned = new Curve();

            foreach (var point in _points)
            {
                cloned.Add(point.Clone());
            }

            return cloned;
        }
    }
}
