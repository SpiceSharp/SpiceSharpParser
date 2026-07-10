namespace SpiceSharpParser
{
    /// <summary>
    /// Compatibility settings for dialect-specific SPICE netlist behavior.
    /// </summary>
    public sealed class CompatibilityOptions
    {
        private CompatibilityOptions(bool isLTspice, bool isPSpice)
        {
            IsLTspice = isLTspice;
            IsPSpice = isPSpice;
        }

        /// <summary>
        /// Gets compatibility options with no dialect-specific behavior enabled.
        /// </summary>
        public static CompatibilityOptions None { get; } = new CompatibilityOptions(false, false);

        /// <summary>
        /// Gets compatibility options for PSpice-generated netlists.
        /// </summary>
        public static CompatibilityOptions PSpice { get; } = new CompatibilityOptions(false, true);

        /// <summary>
        /// Gets compatibility options for LTspice-generated netlists.
        /// </summary>
        public static CompatibilityOptions LTspice { get; } = new CompatibilityOptions(true, false);

        /// <summary>
        /// Gets a value indicating whether LTspice compatibility behavior is enabled.
        /// </summary>
        public bool IsLTspice { get; }

        /// <summary>
        /// Gets a value indicating whether PSpice compatibility behavior is enabled.
        /// </summary>
        public bool IsPSpice { get; }
    }
}
