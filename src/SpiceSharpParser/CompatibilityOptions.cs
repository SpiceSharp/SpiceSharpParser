namespace SpiceSharpParser
{
    /// <summary>
    /// Compatibility settings for dialect-specific SPICE netlist behavior.
    /// </summary>
    public sealed class CompatibilityOptions
    {
        private CompatibilityOptions(bool isLTspice)
        {
            IsLTspice = isLTspice;
        }

        /// <summary>
        /// Gets compatibility options with no dialect-specific behavior enabled.
        /// </summary>
        public static CompatibilityOptions None { get; } = new CompatibilityOptions(false);

        /// <summary>
        /// Gets compatibility options for LTspice-generated netlists.
        /// </summary>
        public static CompatibilityOptions LTspice { get; } = new CompatibilityOptions(true);

        /// <summary>
        /// Gets a value indicating whether LTspice compatibility behavior is enabled.
        /// </summary>
        public bool IsLTspice { get; }
    }
}
