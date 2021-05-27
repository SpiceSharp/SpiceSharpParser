using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// Settings for <see cref="SpiceLexer"/>.
    /// </summary>
    public class SpiceLexerSettings
    {
        public SpiceLexerSettings(SpiceNetlistCaseSensitivitySettings sensitivitySettings)
        {
            IsDotStatementNameCaseSensitive = sensitivitySettings?.IsDotStatementNameCaseSensitive ?? throw new ArgumentNullException(nameof(sensitivitySettings));
        }

        public SpiceLexerSettings(bool isDotStatementNameCaseSensitive)
        {
            IsDotStatementNameCaseSensitive = isDotStatementNameCaseSensitive;
        }

        public SpiceLexerSettings()
        {
            IsDotStatementNameCaseSensitive = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether text has a first line with the title.
        /// </summary>
        public bool HasTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether dot statements names are case-sensitive.
        /// </summary>
        public bool IsDotStatementNameCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether bus syntax is enabled.
        /// </summary>
        public bool EnableBusSyntax { get; set; }
    }
}