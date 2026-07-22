namespace SpiceSharpParser
{
    /// <summary>
    /// Identifies the compatibility dialect used throughout compilation.
    /// </summary>
    public enum SpiceDialect
    {
        /// <summary>
        /// Uses standard SPICE behavior without vendor-specific compatibility rules.
        /// </summary>
        Spice3,

        /// <summary>
        /// Enables PSpice compatibility rules.
        /// </summary>
        PSpice,

        /// <summary>
        /// Enables LTspice compatibility rules.
        /// </summary>
        LTspice,
    }
}
