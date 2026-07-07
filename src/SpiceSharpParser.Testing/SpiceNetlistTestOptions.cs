using System.Text;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.Testing
{
    /// <summary>
    /// Common parser and reader options for SPICE netlist tests.
    /// </summary>
    public sealed class SpiceNetlistTestOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the first line is treated as the title.
        /// </summary>
        public bool HasTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether .END is required.
        /// </summary>
        public bool IsEndRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether a trailing newline is required.
        /// Null keeps the parser default.
        /// </summary>
        public bool? IsNewlineRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SPICE bus syntax is enabled.
        /// </summary>
        public bool EnableBusSyntax { get; set; }

        /// <summary>
        /// Gets or sets the dialect compatibility options used by both parser and reader.
        /// </summary>
        public CompatibilityOptions Compatibility { get; set; } = CompatibilityOptions.None;

        /// <summary>
        /// Gets or sets the optional deterministic random seed.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the optional working directory for includes, libs, and external files.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the encoding used for external files.
        /// </summary>
        public Encoding ExternalFilesEncoding { get; set; } = Encoding.Default;

        /// <summary>
        /// Gets or sets the parser and reader case-sensitivity settings.
        /// </summary>
        public SpiceNetlistCaseSensitivitySettings CaseSensitivity { get; set; } = new SpiceNetlistCaseSensitivitySettings();

        /// <summary>
        /// Gets or sets a value indicating whether parser mappings from SpiceSharpParser.CustomComponents are enabled.
        /// </summary>
        public bool UseCustomComponents { get; set; }

        /// <summary>
        /// Gets or sets the separator for expanded subcircuit object and node names.
        /// </summary>
        public string Separator { get; set; } = ".";

        /// <summary>
        /// Gets or sets a value indicating whether subcircuits should be expanded by the reader.
        /// </summary>
        public bool ExpandSubcircuits { get; set; } = true;
    }
}
