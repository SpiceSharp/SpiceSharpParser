using SpiceSharpParser.Lexers.Netlist.Spice;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    /// <summary>
    /// Case-sensitivity settings for netlist reader.
    /// </summary>
    public class SpiceNetlistCaseSensitivitySettings : ISpiceNetlistCaseSensitivitySettings
    {
        private readonly SpiceLexerSettings _lexerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistCaseSensitivitySettings"/> class.
        /// </summary>
        public SpiceNetlistCaseSensitivitySettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistCaseSensitivitySettings"/> class.
        /// </summary>
        /// <param name="lexerSettings">Lexer settings.</param>
        public SpiceNetlistCaseSensitivitySettings(SpiceLexerSettings lexerSettings)
        {
            _lexerSettings = lexerSettings ?? throw new ArgumentNullException(nameof(lexerSettings));
        }

        /// <summary>
        /// Gets or sets a value indicating whether names are case-sensitive.
        /// </summary>
        public bool IsEntityNamesCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether dot statements names are case-sensitive.
        /// </summary>
        public bool IsDotStatementNameCaseSensitive => _lexerSettings?.IsDotStatementNameCaseSensitive ?? false;

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
    }
}