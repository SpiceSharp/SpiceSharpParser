using System;

namespace SpiceSharpParser.Testing
{
    /// <summary>
    /// Absolute and relative tolerance used by numeric test helpers.
    /// </summary>
    public readonly struct TestTolerance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestTolerance"/> struct.
        /// </summary>
        /// <param name="absolute">The absolute tolerance.</param>
        /// <param name="relative">The relative tolerance.</param>
        public TestTolerance(double absolute, double relative)
        {
            this.Absolute = absolute;
            this.Relative = relative;
        }

        /// <summary>
        /// Gets the default tolerance used by legacy integration tests.
        /// </summary>
        public static TestTolerance Default { get; } = new TestTolerance(1e-12, 1e-3);

        /// <summary>
        /// Gets the absolute tolerance.
        /// </summary>
        public double Absolute { get; }

        /// <summary>
        /// Gets the relative tolerance.
        /// </summary>
        public double Relative { get; }

        /// <summary>
        /// Checks whether expected and actual are within tolerance.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <returns>True when the values are close enough.</returns>
        public bool Equals(double expected, double actual)
        {
            return Math.Abs(expected - actual) <= this.ValueFor(expected, actual);
        }

        /// <summary>
        /// Calculates the effective tolerance for a value pair.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <returns>The effective tolerance.</returns>
        public double ValueFor(double expected, double actual)
        {
            return (Math.Max(Math.Abs(actual), Math.Abs(expected)) * this.Relative) + this.Absolute;
        }
    }
}
