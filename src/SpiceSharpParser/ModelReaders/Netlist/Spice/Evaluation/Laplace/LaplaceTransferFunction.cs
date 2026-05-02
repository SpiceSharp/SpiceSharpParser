using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace
{
    internal sealed class LaplaceTransferFunction
    {
        private readonly Polynomial _numerator;
        private readonly Polynomial _denominator;
        private readonly double[] _numeratorCoefficients;
        private readonly double[] _denominatorCoefficients;

        public LaplaceTransferFunction(Polynomial numerator, Polynomial denominator)
        {
            _numerator = numerator ?? throw new ArgumentNullException(nameof(numerator));
            _denominator = denominator ?? throw new ArgumentNullException(nameof(denominator));

            if (_denominator.IsZero)
            {
                throw new LaplaceExpressionException("laplace transfer denominator cannot be zero");
            }

            _numeratorCoefficients = _numerator.ToArray();
            _denominatorCoefficients = _denominator.ToArray();
        }

        public IReadOnlyList<double> Numerator => _numeratorCoefficients;

        public IReadOnlyList<double> Denominator => _denominatorCoefficients;

        public double[] NumeratorCoefficients => Copy(_numeratorCoefficients);

        public double[] DenominatorCoefficients => Copy(_denominatorCoefficients);

        public int Order => _denominator.Degree;

        public LaplaceTransferFunction ScaleNumerator(double factor)
        {
            return new LaplaceTransferFunction(_numerator.Scale(factor), _denominator);
        }

        public Complex EvaluateComplex(Complex value)
        {
            return _numerator.EvaluateComplex(value) / _denominator.EvaluateComplex(value);
        }

        private static double[] Copy(double[] values)
        {
            var copy = new double[values.Length];
            Array.Copy(values, copy, values.Length);
            return copy;
        }
    }
}
