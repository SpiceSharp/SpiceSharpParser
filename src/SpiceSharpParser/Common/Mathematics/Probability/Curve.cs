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
        /// <summary>
        /// Initializes a new instance of the <see cref="Curve"/> class.
        /// </summary>
        public Curve()
        {
            Points = new List<Point>();
        }

        public List<Point> Points { get; protected set; }

        /// <summary>
        /// Gets the count of points.
        /// </summary>
        public int PointsCount => Points.Count;

        /// <summary>
        /// Gets or sets a point.
        /// </summary>
        /// <param name="index">Point index.</param>
        /// <returns>
        /// A curve's point.
        /// </returns>
        public Point this[int index] => Points[index];

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

            for (var i = 0; i < Points.Count; i++)
            {
                if (Points[i].X > point.X)
                {
                    Points.Insert(i, point);
                    return;
                }
            }

            Points.Add(point);
        }

        /// <summary>
        /// Computes the area under the curve.
        /// </summary>
        /// <returns>
        /// The area under curve.
        /// </returns>
        public double ComputeAreaUnderCurve()
        {
            return ComputeAreaUnderCurve(Points.Count);
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
            if (limitPointIndex > Points.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(limitPointIndex));
            }

            double result = 0.0;

            for (var i = 1; i < limitPointIndex; i++)
            {
                result += (Points[i].X - Points[i - 1].X) * (Points[i].Y + Points[i - 1].Y) / 2.0;
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
            while (i < Points.Count - 1)
            {
                if (Points[i + 1].X > x)
                {
                    break;
                }

                i++;
            }

            for (var j = 1; j < i + 1; j++)
            {
                result += (Points[j].X - Points[j - 1].X) * (Points[j].Y + Points[j - 1].Y) / 2.0;
            }

            if (i < Points.Count - 1)
            {
                var nextPoint = Points.Skip(i + 1).SkipWhile(r => r.X == Points[i].X).FirstOrDefault();

                if (nextPoint != null)
                {
                    var yDiff = (nextPoint.Y - Points[i].Y) / (nextPoint.X - Points[i].X) *
                                Math.Abs(x - Points[i].X);

                    result += Math.Abs(x - Points[i].X) * ((2 * Points[i].Y) + yDiff) / 2.0;
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
            Points.ForEach(p => p.Y *= scaleFactor);
        }

        /// <summary>
        /// Gets the first point.
        /// </summary>
        /// <returns>
        /// The first point.
        /// </returns>
        public Point GetFirstPoint()
        {
            return Points.FirstOrDefault();
        }

        /// <summary>
        /// Gets the last point.
        /// </summary>
        /// <returns>
        /// The last point.
        /// </returns>
        public Point GetLastPoint()
        {
            return Points.LastOrDefault();
        }

        /// <summary>
        /// Clears the points.
        /// </summary>
        public void Clear()
        {
            Points.Clear();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator of points.
        /// </returns>
        public IEnumerator<Point> GetEnumerator()
        {
            return Points.GetEnumerator();
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

            foreach (var point in Points)
            {
                cloned.Add(point.Clone());
            }

            return cloned;
        }
    }
}