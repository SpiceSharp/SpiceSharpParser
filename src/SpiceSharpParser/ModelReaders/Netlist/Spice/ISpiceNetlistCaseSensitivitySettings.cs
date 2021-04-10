namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public interface ISpiceNetlistCaseSensitivitySettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether dot statements names are case-sensitive.
        /// </summary>
        bool IsDotStatementNameCaseSensitive { get; }

        /// <summary>
        /// Gets or sets a value indicating whether entity names are case-sensitive.
        /// </summary>
        bool IsEntityNamesCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether model types names are case-sensitive.
        /// </summary>
        bool IsModelTypeCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether function names are case-sensitive.
        /// </summary>
        bool IsFunctionNameCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether random distribution names are case-sensitive.
        /// </summary>
        bool IsDistributionNameCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether parameter names are case-sensitive.
        /// </summary>
        bool IsParameterNameCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether expression names are case-sensitive.
        /// </summary>
        bool IsExpressionNameCaseSensitive { get; set; }
    }
}