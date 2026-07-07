using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements;
using Xunit;

namespace SpiceSharpParser.Testing
{
    /// <summary>
    /// Shared assertions for parser validation, measurements, and numeric tolerances.
    /// </summary>
    public static class SpiceNetlistAssertions
    {
        /// <summary>
        /// Asserts that validation contains neither errors nor warnings.
        /// </summary>
        /// <param name="validation">The validation collection.</param>
        public static void AssertNoValidationIssues(ValidationEntryCollection validation)
        {
            var messages = ValidationMessages(validation);
            Assert.False(validation.HasError, "Unexpected validation error: " + messages);
            Assert.False(validation.HasWarning, "Unexpected validation warning: " + messages);
        }

        /// <summary>
        /// Asserts that a model has no validation errors.
        /// </summary>
        /// <param name="model">The model.</param>
        public static void AssertNoValidationErrors(SpiceSharpModel model)
        {
            AssertNoValidationErrors(model.ValidationResult);
        }

        /// <summary>
        /// Asserts that validation contains no errors.
        /// </summary>
        /// <param name="validation">The validation collection.</param>
        public static void AssertNoValidationErrors(ValidationEntryCollection validation)
        {
            Assert.False(validation.HasError, ValidationMessages(validation));
        }

        /// <summary>
        /// Asserts that a validation error contains text.
        /// </summary>
        /// <param name="validation">The validation collection.</param>
        /// <param name="expected">The expected text.</param>
        public static void AssertErrorContains(ValidationEntryCollection validation, string expected)
        {
            Assert.Contains(expected, string.Join(Environment.NewLine, validation.Errors.Select(error => error.Message)), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Asserts that a validation warning contains text.
        /// </summary>
        /// <param name="validation">The validation collection.</param>
        /// <param name="expected">The expected text.</param>
        public static void AssertWarningContains(ValidationEntryCollection validation, string expected)
        {
            Assert.Contains(expected, string.Join(Environment.NewLine, validation.Warnings.Select(warning => warning.Message)), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Formats all validation messages.
        /// </summary>
        /// <param name="validation">The validation collection.</param>
        /// <returns>The formatted messages.</returns>
        public static string ValidationMessages(ValidationEntryCollection validation)
        {
            return string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
        }

        /// <summary>
        /// Asserts that a measurement exists, succeeded, and matches an expected value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="name">The measurement name.</param>
        /// <param name="expectedValue">The expected value.</param>
        public static void AssertMeasurement(SpiceSharpModel model, string name, double expectedValue)
        {
            AssertMeasurement(model, name, expectedValue, TestTolerance.Default);
        }

        /// <summary>
        /// Asserts that a measurement exists, succeeded, and matches an expected value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="name">The measurement name.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="tolerance">The numeric tolerance.</param>
        public static void AssertMeasurement(SpiceSharpModel model, string name, double expectedValue, TestTolerance tolerance)
        {
            var result = AssertMeasurementSuccess(model, name);
            AssertClose(expectedValue, result.Value, tolerance, "Measurement '" + name + "'");
        }

        /// <summary>
        /// Asserts that a measurement exists and succeeded.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="name">The measurement name.</param>
        /// <returns>The first measurement result.</returns>
        public static MeasurementResult AssertMeasurementSuccess(SpiceSharpModel model, string name)
        {
            Assert.True(model.Measurements.ContainsKey(name), "Measurement '" + name + "' not found");
            var results = model.Measurements[name];
            Assert.True(results.Count > 0, "Measurement '" + name + "' has no results");
            Assert.True(results[0].Success, "Measurement '" + name + "' failed");
            return results[0];
        }

        /// <summary>
        /// Asserts that one Fourier result exists and succeeded.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The Fourier result.</returns>
        public static FourierAnalysisResult AssertSingleSuccessfulFourierResult(SpiceSharpModel model)
        {
            Assert.Single(model.FourierAnalyses);
            var result = model.FourierAnalyses[0];
            Assert.True(result.Success, result.ErrorMessage);
            return result;
        }

        /// <summary>
        /// Asserts that two values are close using the default tolerance.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AssertClose(double expected, double actual)
        {
            AssertClose(expected, actual, TestTolerance.Default);
        }

        /// <summary>
        /// Asserts that two values are close using an absolute tolerance.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="absoluteTolerance">The absolute tolerance.</param>
        public static void AssertClose(double expected, double actual, double absoluteTolerance)
        {
            AssertClose(expected, actual, new TestTolerance(absoluteTolerance, 0.0));
        }

        /// <summary>
        /// Asserts that two values are close using the supplied tolerance.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="tolerance">The numeric tolerance.</param>
        /// <param name="label">The optional assertion label.</param>
        public static void AssertClose(double expected, double actual, TestTolerance tolerance, string label = null)
        {
            Assert.True(
                tolerance.Equals(expected, actual),
                FormatCloseMessage(expected, actual, tolerance, label));
        }

        /// <summary>
        /// Checks that all exported samples match a reference function.
        /// </summary>
        /// <param name="exports">The time/frequency/sweep value samples.</param>
        /// <param name="reference">The reference function.</param>
        /// <returns>True when all samples are within tolerance.</returns>
        public static bool EqualsWithTolerance(IEnumerable<Tuple<double, double>> exports, Func<double, double> reference)
        {
            return EqualsWithTolerance(exports, reference, TestTolerance.Default);
        }

        /// <summary>
        /// Checks that all exported samples match a reference function.
        /// </summary>
        /// <param name="exports">The time/frequency/sweep value samples.</param>
        /// <param name="reference">The reference function.</param>
        /// <param name="tolerance">The numeric tolerance.</param>
        /// <returns>True when all samples are within tolerance.</returns>
        public static bool EqualsWithTolerance(
            IEnumerable<Tuple<double, double>> exports,
            Func<double, double> reference,
            TestTolerance tolerance)
        {
            foreach (var export in exports)
            {
                if (!tolerance.Equals(reference(export.Item1), export.Item2))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks that values match references pairwise.
        /// </summary>
        /// <param name="exports">The actual values.</param>
        /// <param name="references">The expected values.</param>
        /// <returns>True when all paired values are within tolerance.</returns>
        public static bool EqualsWithTolerance(IEnumerable<double> exports, IEnumerable<double> references)
        {
            return EqualsWithTolerance(exports, references, TestTolerance.Default);
        }

        /// <summary>
        /// Checks that values match references pairwise.
        /// </summary>
        /// <param name="exports">The actual values.</param>
        /// <param name="references">The expected values.</param>
        /// <param name="tolerance">The numeric tolerance.</param>
        /// <returns>True when all paired values are within tolerance.</returns>
        public static bool EqualsWithTolerance(
            IEnumerable<double> exports,
            IEnumerable<double> references,
            TestTolerance tolerance)
        {
            using (var exportIt = exports.GetEnumerator())
            using (var referencesIt = references.GetEnumerator())
            {
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    if (!tolerance.Equals(referencesIt.Current, exportIt.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks that exported samples match references pairwise.
        /// </summary>
        /// <param name="exports">The actual samples.</param>
        /// <param name="references">The expected values.</param>
        /// <returns>True when all paired values are within tolerance.</returns>
        public static bool EqualsWithTolerance(IEnumerable<Tuple<double, double>> exports, IEnumerable<double> references)
        {
            return EqualsWithTolerance(exports.Select(export => export.Item2), references, TestTolerance.Default);
        }

        private static string FormatCloseMessage(double expected, double actual, TestTolerance tolerance, string label)
        {
            var prefix = string.IsNullOrEmpty(label) ? string.Empty : label + ": ";
            return prefix + "expected " + expected.ToString("R") + ", got " + actual.ToString("R") + " (tolerance " + tolerance.ValueFor(expected, actual).ToString("R") + ")";
        }
    }
}
