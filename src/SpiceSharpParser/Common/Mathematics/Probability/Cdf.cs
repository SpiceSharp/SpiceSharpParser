using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Cumulative distribution function.
    /// </summary>
    public class Cdf : Curve
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cdf"/> class.
        /// </summary>
        /// <param name="pdfCurve">Pdf.</param>
        /// <param name="numberOfPoints">Number of cdf points.</param>
        public Cdf(Pdf pdfCurve, int numberOfPoints)
        {
            if (pdfCurve == null)
            {
                throw new ArgumentNullException(nameof(pdfCurve));
            }

            double xFirst = pdfCurve.GetFirstPoint().X;
            double xLast = pdfCurve.GetLastPoint().X;

            Add(new Point(xFirst, 0.0));

            for (var i = 0; i < numberOfPoints - 1; i++)
            {
                double x = xFirst + (((xLast - xFirst) / (numberOfPoints - 1)) * (i + 1));
                Add(new Point(x, pdfCurve.ComputeAreaUnderCurve(x)));
            }
        }
    }
}
