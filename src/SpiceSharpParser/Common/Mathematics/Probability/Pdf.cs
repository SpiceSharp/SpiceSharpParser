using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Probability density function.
    /// </summary>
    public class Pdf : Curve
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pdf"/> class.
        /// </summary>
        public Pdf()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdf"/> class.
        /// </summary>
        /// <param name="curve">Pdf curve.</param>
        public Pdf(Curve curve)
        {
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            var probabilityCurve = curve.Clone();
            var area = probabilityCurve.ComputeAreaUnderCurve();

            if (area == 0.0)
            {
                throw new ArgumentException(nameof(curve));
            }

            if (area != 1.0)
            {
                probabilityCurve.ScaleY(1.0 / area);
            }

            foreach (var point in probabilityCurve)
            {
                Add(point);
            }
        }

        /// <summary>
        /// Validates the pdf.
        /// </summary>
        public void Validate()
        {
            var area = ComputeAreaUnderCurve();
            if (area != 1.0)
            {
                throw new Exception("Pdf is invalid");
            }
        }
    }
}
