using System;
using System.Text;
using SpiceSharpParser.Common;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Configures parsing, preprocessing, translation, and linting performed by <see cref="SpiceCompiler"/>.
    /// </summary>
    public sealed class SpiceCompileOptions
    {
        /// <summary>
        /// Gets or sets the dialect applied consistently to the parser and reader.
        /// </summary>
        public SpiceDialect Dialect { get; set; } = SpiceDialect.Spice3;

        /// <summary>
        /// Gets or sets the working directory used to resolve includes, libraries, and external resources.
        /// CompileFile uses the source file's directory when this value is null.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lexer and parser recovery is enabled and later stages
        /// should run when an earlier stage produced errors but still returned a usable intermediate model.
        /// </summary>
        public bool ContinueAfterErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of lexer and parser errors collected before recovery stops.
        /// </summary>
        public int MaximumSyntaxErrors { get; set; } = 25;

        /// <summary>
        /// Gets or sets a value indicating whether structural linting is run after translation.
        /// </summary>
        public bool RunLinter { get; set; } = true;

        /// <summary>
        /// Gets or sets the policy used to suppress or reclassify diagnostics for CI and editor consumers.
        /// Policy does not change whether <see cref="SpiceCompilationResult.Model"/> is safe to expose.
        /// </summary>
        public SpiceDiagnosticPolicy DiagnosticPolicy { get; set; } = new SpiceDiagnosticPolicy();

        /// <summary>
        /// Gets or sets a value indicating whether CompileFile rethrows source-file access failures.
        /// The default is to return an SSP2001 or SSP2002 diagnostic.
        /// </summary>
        public bool ThrowOnFileAccessError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the first source line is the netlist title.
        /// </summary>
        public bool HasTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the netlist must contain an .END statement.
        /// </summary>
        public bool IsEndRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the source must end with a newline. Null preserves the parser default.
        /// </summary>
        public bool? IsNewlineRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SPICE bus syntax is enabled.
        /// </summary>
        public bool EnableBusSyntax { get; set; }

        /// <summary>
        /// Gets or sets the encoding used for source and external files.
        /// </summary>
        public Encoding ExternalFilesEncoding { get; set; } = Encoding.Default;

        /// <summary>
        /// Gets or sets the case-sensitivity rules shared by the parser and reader.
        /// </summary>
        public SpiceNetlistCaseSensitivitySettings CaseSensitivity { get; set; } = new SpiceNetlistCaseSensitivitySettings();

        /// <summary>
        /// Gets or sets the optional deterministic evaluator seed.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the separator for expanded subcircuit object and node names.
        /// </summary>
        public string Separator { get; set; } = ".";

        /// <summary>
        /// Gets or sets a value indicating whether subcircuits are expanded during translation.
        /// </summary>
        public bool ExpandSubcircuits { get; set; } = true;

        /// <summary>
        /// Gets or sets an optional parser-settings customization callback.
        /// Dialect and working directory are re-applied afterward to keep both stages consistent.
        /// </summary>
        public Action<SpiceNetlistParserSettings> ConfigureParser { get; set; }

        /// <summary>
        /// Gets or sets an optional reader-settings customization callback, for example to add custom mappings.
        /// Dialect is re-applied afterward to keep both stages consistent.
        /// </summary>
        public Action<SpiceNetlistReaderSettings> ConfigureReader { get; set; }
    }
}
