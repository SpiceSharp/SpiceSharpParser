namespace SpiceSharpParser.Common
{
    /// <summary>
    /// Case-sensitivity settings.
    /// </summary>
    public class SpiceNetlistCaseSensitivitySettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether names are case-sensitive.
        /// </summary>
        public bool IsEntityNamesCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether dot statements names are case-sensitive.
        /// </summary>
        public bool IsDotStatementNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether model types names are case-sensitive.
        /// </summary>
        public bool IsModelTypeCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether node names are case-sensitive.
        /// </summary>
        public bool IsNodeNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether function names are case-sensitive.
        /// </summary>
        public bool IsFunctionNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether random distribution names are case-sensitive.
        /// </summary>
        public bool IsDistributionNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether parameter names are case-sensitive.
        /// </summary>
        public bool IsParameterNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether expression names are case-sensitive.
        /// </summary>
        public bool IsExpressionNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether subcircuit names are case-sensitive.
        /// </summary>
        public bool IsSubcircuitNameCaseSensitive { get; set; } = false;
    }
}