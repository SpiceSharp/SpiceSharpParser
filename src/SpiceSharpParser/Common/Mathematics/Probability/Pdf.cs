using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class Pdf : Curve
    {
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
    }
}
