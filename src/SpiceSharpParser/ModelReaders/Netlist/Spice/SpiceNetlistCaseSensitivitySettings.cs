namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    /// <summary>
    /// Case-sensitivity settings for netlist reader.
    /// </summary>
    public class SpiceNetlistCaseSensitivitySettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether entity names are case-sensitive.
        /// </summary>
        public bool IsEntityNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether entity parameter names are case-sensitive.
        /// </summary>
        public bool IsEntityParameterNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether node names are case-sensitive.
        /// </summary>
        public bool IsNodeNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether function names are case-sensitive.
        /// </summary>
        public bool IsFunctionNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether parameter names are case-sensitive.
        /// </summary>
        public bool IsParameterNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether let names are case-sensitive.
        /// </summary>
        public bool IsLetExpressionNameCaseSensitive { get; set; } = false;
    }
}
