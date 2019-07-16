using System;

namespace SpiceSharpParser.Common.Mathematics.Probability.Pdfs
{
    /// <summary>
    /// Normal distribution with 0 mean.
    /// </summary>
    public class NormalPdf : Pdf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalPdf"/> class.
        /// </summary>
        /// <param name="evaluationPoints">Number of evaluation points.</param>
        /// <param name="limit">Pdf limit.</param>
        public NormalPdf(int evaluationPoints, double limit)
        {
            double xMin = -limit;
            double xMax = limit;
            double step = (xMax - xMin) / evaluationPoints;

            double currentX = xMin;
            for (var i = 0; i < evaluationPoints; i++)
            {
                if (currentX > xMax)
                {
                    currentX = xMax;
                }

                Add(new Point(currentX, Compute(currentX)));
                currentX += step;
            }

            ScaleY(1.0 / ComputeAreaUnderCurve());
        }

        private double Compute(double currentX)
        {
            return (1.0 / Math.Sqrt(2.0 * Math.PI)) * Math.Exp(-(currentX * currentX) / 2.0);
        }
    }
}
