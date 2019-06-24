using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class Cdf : Curve
    {
        public Cdf(Pdf pdfCurve)
        {
            if (pdfCurve == null)
            {
                throw new ArgumentNullException(nameof(pdfCurve));
            }

            for (var i = 0; i < pdfCurve.PointsCount; i++)
            {
                Add(new Point(pdfCurve[i].X, pdfCurve.ComputeAreaUnderCurve(i + 1)));
            }
        }
    }
}
