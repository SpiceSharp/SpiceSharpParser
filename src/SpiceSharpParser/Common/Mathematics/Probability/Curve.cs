using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Curve.
    /// </summary>
    public class Curve : IEnumerable<Point>
    {
        private readonly List<Point> _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="Curve"/> class.
        /// </summary>
        public Curve()
        {
            _points = new List<Point>();
        }

        /// <summary>
        /// Gets the count of points.
        /// </summary>
        public int PointsCount => _points.Count;

        /// <summary>
        /// Gets or sets a point.
        /// </summary>
        /// <param name="index">Point index.</param>
        /// <returns>
        /// A curve's point.
        /// </returns>
        public Point this[int index] => _points[index];

        /// <summary>
        /// Adds a point to the curve.
        /// </summary>
        /// <param name="point">A point.</param>
        public void Add(Point point)
        {
            if (point == null)
            {
                throw new ArgumentNullException(nameof(point));
            }

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

        /// <summary>
        /// Computes the area under the curve.
        /// </summary>
        /// <returns>
        /// The area under curve.
        /// </returns>
        public double ComputeAreaUnderCurve()
        {
            return ComputeAreaUnderCurve(_points.Count);
        }

        /// <summary>
        /// Computes the area under the curve.
        /// </summary>
        /// <param name="limitPointIndex">Index of last point.</param>
        /// <returns>
        /// The area under curve.
        /// </returns>
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

        /// <summary>
        /// Computes the area under the curve.
        /// </summary>
        /// <param name="x">X value of last point.</param>
        /// <returns>
        /// The area under curve.
        /// </returns>
        public double ComputeAreaUnderCurve(double x)
        {
            double result = 0.0;

            var i = 0;
            while (i < _points.Count - 1)
            {
                if (_points[i + 1].X > x)
                {
                    break;
                }

                i++;
            }

            for (var j = 1; j < i + 1; j++)
            {
                result += (_points[j].X - _points[j-1].X) * (_points[j].Y + _points[j-1].Y) / 2.0;
            }

            if (i < _points.Count - 1)
            {
                var nextPoint = _points.Skip(i+1).SkipWhile(r => r.X == _points[i].X).FirstOrDefault();

                if (nextPoint != null)
                {
                    var yDiff = (nextPoint.Y - _points[i].Y) / (nextPoint.X - _points[i].X) *
                                Math.Abs(x - _points[i].X);

                    result += Math.Abs(x - _points[i].X) * ((2 * _points[i].Y) + yDiff) / 2.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Scales the curve.
        /// </summary>
        /// <param name="scaleFactor">Scaling factor.</param>
        public void ScaleY(double scaleFactor)
        {
            _points.ForEach(p => p.Y *= scaleFactor);
        }

        /// <summary>
        /// Gets the first point.
        /// </summary>
        /// <returns>
        /// The first point.
        /// </returns>
        public Point GetFirstPoint()
        {
            return _points.FirstOrDefault();
        }

        /// <summary>
        /// Gets the last point.
        /// </summary>
        /// <returns>
        /// The last point.
        /// </returns>
        public Point GetLastPoint()
        {
            return _points.LastOrDefault();
        }

        /// <summary>
        /// Clears the points.
        /// </summary>
        public void Clear()
        {
            _points.Clear();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator of points.
        /// </returns>
        public IEnumerator<Point> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Clones the curve.
        /// </summary>
        /// <returns>
        /// A cloned curve.
        /// </returns>
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
