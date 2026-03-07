using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public static class LaplaceSourceHelper
    {
        public static IEntity CreateLaplaceVoltageSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context,
            LaplaceParameter lp)
        {
            var inputNodes = ParseInputNodes(lp.InputExpression);
            ExtractPolynomialCoefficients(lp.TransferFunction, context, out double[] numerator, out double[] denominator);

            var entity = new LaplaceVoltageControlledVoltageSource(name);

            // Build 4-node parameter collection: out+, out-, ctrl+, ctrl-
            var nodeParams = CreateNodeParameters(parameters, inputNodes);
            context.CreateNodes(entity, nodeParams);
            entity.Parameters.Numerator = numerator;
            entity.Parameters.Denominator = denominator;
            return entity;
        }

        public static IEntity CreateLaplaceCurrentSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context,
            LaplaceParameter lp)
        {
            var inputNodes = ParseInputNodes(lp.InputExpression);
            ExtractPolynomialCoefficients(lp.TransferFunction, context, out double[] numerator, out double[] denominator);

            var entity = new LaplaceVoltageControlledCurrentSource(name);

            // Build 4-node parameter collection: out+, out-, ctrl+, ctrl-
            var nodeParams = CreateNodeParameters(parameters, inputNodes);
            context.CreateNodes(entity, nodeParams);
            entity.Parameters.Numerator = numerator;
            entity.Parameters.Denominator = denominator;
            return entity;
        }

        private static ParameterCollection CreateNodeParameters(ParameterCollection parameters, string[] inputNodes)
        {
            // First 2 parameters are output nodes, then add control nodes from parsed input expression
            var nodeParams = new ParameterCollection(new List<Parameter>());
            nodeParams.Add(parameters[0]); // out+
            nodeParams.Add(parameters[1]); // out-
            nodeParams.Add(new WordParameter(inputNodes[0], null));  // ctrl+
            nodeParams.Add(new WordParameter(inputNodes[1], null));  // ctrl-
            return nodeParams;
        }

        private static string[] ParseInputNodes(string inputExpression)
        {
            // Parse V(node) or V(node1, node2) from input expression
            var match = Regex.Match(inputExpression, @"[Vv]\(([^,\)]+)(?:,\s*([^)]+))?\)");
            if (match.Success)
            {
                var node1 = match.Groups[1].Value.Trim();
                var node2 = match.Groups[2].Success ? match.Groups[2].Value.Trim() : "0";
                return new[] { node1, node2 };
            }

            return new[] { inputExpression.Trim(), "0" };
        }

        private static void ExtractPolynomialCoefficients(
            string transferFunction,
            IReadingContext context,
            out double[] numerator,
            out double[] denominator)
        {
            // Use numerical evaluation to extract polynomial coefficients.
            // Evaluate H(s) at multiple s values and solve for coefficients.
            // For a rational polynomial H(s) = N(s)/D(s), we sample at enough
            // points to determine the coefficients.
            var evaluator = context.Evaluator;

            // First, try to determine polynomial degree by probing
            int maxDegree = DetermineMaxDegree(transferFunction, evaluator);

            // Sample the transfer function at s = 0, 1, 2, ..., 2*maxDegree
            int numSamples = 2 * maxDegree + 1;
            double[] sValues = new double[numSamples];
            double[] hValues = new double[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                sValues[i] = i;
                hValues[i] = EvaluateAtS(transferFunction, sValues[i], evaluator);
            }

            // For simple cases, try direct coefficient extraction
            if (TryExtractSimpleRational(transferFunction, evaluator, out numerator, out denominator))
            {
                return;
            }

            // Fallback: treat as a constant gain
            double dcGain = EvaluateAtS(transferFunction, 0, evaluator);
            numerator = new double[] { dcGain };
            denominator = new double[] { 1.0 };
        }

        private static bool TryExtractSimpleRational(
            string transferFunction,
            Common.Evaluation.IEvaluator evaluator,
            out double[] numerator,
            out double[] denominator)
        {
            // Evaluate at multiple points and use the Prony-like approach
            // to extract rational polynomial coefficients.
            // H(s) = (n0 + n1*s + n2*s^2 + ...) / (d0 + d1*s + d2*s^2 + ...)
            // We normalize d0 = 1 (or the leading denominator coefficient).

            // Try increasing orders until we find a good fit
            for (int numOrder = 0; numOrder <= 4; numOrder++)
            {
                for (int denOrder = 0; denOrder <= 4; denOrder++)
                {
                    if (numOrder == 0 && denOrder == 0)
                    {
                        // Just a constant
                        double h0 = EvaluateAtS(transferFunction, 0, evaluator);
                        if (IsGoodFit(transferFunction, evaluator, new double[] { h0 }, new double[] { 1.0 }))
                        {
                            numerator = new double[] { h0 };
                            denominator = new double[] { 1.0 };
                            return true;
                        }
                        continue;
                    }

                    int totalCoeffs = (numOrder + 1) + denOrder; // d0=1 is fixed
                    int numSamples = totalCoeffs + 2; // over-determined

                    // Solve the linear system: H(s_i) * D(s_i) = N(s_i)
                    // H(s_i) * (1 + d1*s_i + d2*s_i^2 + ...) = n0 + n1*s_i + n2*s_i^2 + ...
                    // Rearranging: n0 + n1*s_i + ... - H(s_i)*d1*s_i - H(s_i)*d2*s_i^2 - ... = H(s_i)

                    var A = new double[numSamples, totalCoeffs];
                    var b = new double[numSamples];

                    for (int i = 0; i < numSamples; i++)
                    {
                        double s = (i + 1) * 0.1; // Use small real values
                        double h = EvaluateAtS(transferFunction, s, evaluator);
                        b[i] = h;

                        int col = 0;
                        // Numerator coefficients: n0, n1*s, n2*s^2, ...
                        for (int j = 0; j <= numOrder; j++)
                        {
                            A[i, col++] = Math.Pow(s, j);
                        }
                        // Denominator coefficients (skip d0=1): -H*d1*s, -H*d2*s^2, ...
                        for (int j = 1; j <= denOrder; j++)
                        {
                            A[i, col++] = -h * Math.Pow(s, j);
                        }
                    }

                    if (SolveLeastSquares(A, b, numSamples, totalCoeffs, out double[] coeffs))
                    {
                        var num = new double[numOrder + 1];
                        var den = new double[denOrder + 1];
                        den[0] = 1.0;

                        int idx = 0;
                        for (int j = 0; j <= numOrder; j++)
                            num[j] = coeffs[idx++];
                        for (int j = 1; j <= denOrder; j++)
                            den[j] = coeffs[idx++];

                        if (IsGoodFit(transferFunction, evaluator, num, den))
                        {
                            numerator = num;
                            denominator = den;
                            return true;
                        }
                    }
                }
            }

            numerator = null;
            denominator = null;
            return false;
        }

        private static bool IsGoodFit(
            string transferFunction,
            Common.Evaluation.IEvaluator evaluator,
            double[] numerator,
            double[] denominator)
        {
            // Check at several test points
            double[] testPoints = { 0.0, 0.5, 1.0, 2.0, 5.0, 10.0 };
            foreach (double s in testPoints)
            {
                double expected = EvaluateAtS(transferFunction, s, evaluator);
                double numVal = 0, denVal = 0;
                for (int i = 0; i < numerator.Length; i++)
                    numVal += numerator[i] * Math.Pow(s, i);
                for (int i = 0; i < denominator.Length; i++)
                    denVal += denominator[i] * Math.Pow(s, i);

                if (Math.Abs(denVal) < 1e-15) continue;
                double actual = numVal / denVal;

                double absErr = Math.Abs(expected - actual);
                double relErr = Math.Abs(expected) > 1e-15 ? absErr / Math.Abs(expected) : absErr;
                if (relErr > 1e-6 && absErr > 1e-12)
                    return false;
            }
            return true;
        }

        private static bool SolveLeastSquares(double[,] A, double[] b, int rows, int cols, out double[] result)
        {
            // Solve A*x = b using normal equations: A^T*A*x = A^T*b
            var ATA = new double[cols, cols];
            var ATb = new double[cols];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < rows; k++)
                        sum += A[k, i] * A[k, j];
                    ATA[i, j] = sum;
                }
                double sum2 = 0;
                for (int k = 0; k < rows; k++)
                    sum2 += A[k, i] * b[k];
                ATb[i] = sum2;
            }

            // Solve using Gaussian elimination with partial pivoting
            result = new double[cols];
            var augmented = new double[cols, cols + 1];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < cols; j++)
                    augmented[i, j] = ATA[i, j];
                augmented[i, cols] = ATb[i];
            }

            for (int col = 0; col < cols; col++)
            {
                // Partial pivoting
                int maxRow = col;
                for (int row = col + 1; row < cols; row++)
                {
                    if (Math.Abs(augmented[row, col]) > Math.Abs(augmented[maxRow, col]))
                        maxRow = row;
                }
                for (int j = col; j <= cols; j++)
                {
                    var tmp = augmented[col, j];
                    augmented[col, j] = augmented[maxRow, j];
                    augmented[maxRow, j] = tmp;
                }

                if (Math.Abs(augmented[col, col]) < 1e-15)
                    return false;

                // Eliminate
                for (int row = col + 1; row < cols; row++)
                {
                    double factor = augmented[row, col] / augmented[col, col];
                    for (int j = col; j <= cols; j++)
                        augmented[row, j] -= factor * augmented[col, j];
                }
            }

            // Back substitution
            for (int i = cols - 1; i >= 0; i--)
            {
                result[i] = augmented[i, cols];
                for (int j = i + 1; j < cols; j++)
                    result[i] -= augmented[i, j] * result[j];
                result[i] /= augmented[i, i];
            }

            return true;
        }

        private static int DetermineMaxDegree(string transferFunction, Common.Evaluation.IEvaluator evaluator)
        {
            // Count occurrences of 's' multiplied together to estimate degree
            // Simple heuristic: count 's' occurrences as upper bound
            int count = 0;
            for (int i = 0; i < transferFunction.Length; i++)
            {
                if (transferFunction[i] == 's' && (i == 0 || !char.IsLetterOrDigit(transferFunction[i - 1]))
                    && (i == transferFunction.Length - 1 || !char.IsLetterOrDigit(transferFunction[i + 1])))
                {
                    count++;
                }
            }
            return Math.Max(count, 1);
        }

        private static double EvaluateAtS(string transferFunction, double sValue, Common.Evaluation.IEvaluator evaluator)
        {
            // Replace standalone 's' with the numeric value and evaluate
            string expr = ReplaceVariable(transferFunction, "s", sValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return evaluator.EvaluateDouble(expr);
        }

        private static string ReplaceVariable(string expression, string variable, string replacement)
        {
            // Replace standalone variable (not part of larger identifier)
            return Regex.Replace(expression, @"(?<![a-zA-Z_])" + Regex.Escape(variable) + @"(?![a-zA-Z_0-9])", replacement);
        }
    }
}
