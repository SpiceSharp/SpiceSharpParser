using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Contains intermediate models and all diagnostics produced by a compiler invocation.
    /// </summary>
    public sealed class SpiceCompilationResult
    {
        internal SpiceCompilationResult(
            SpiceNetlist inputModel,
            SpiceNetlist expandedModel,
            SpiceSharpModel model,
            IEnumerable<SpiceDiagnostic> diagnostics,
            SpiceDialect effectiveDialect,
            IEnumerable<SpiceDependency> dependencies = null,
            SpiceDiagnosticPolicy diagnosticPolicy = null)
        {
            InputModel = inputModel;
            ExpandedModel = expandedModel;
            AllDiagnostics = diagnostics.ToList().AsReadOnly();
            Dependencies = (dependencies ?? Enumerable.Empty<SpiceDependency>()).ToList().AsReadOnly();
            EffectiveDialect = effectiveDialect;
            TranslatedModel = model;
            Model = AllDiagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) ? null : model;
            Compatibility = new CompatibilityReport(AllDiagnostics, Model != null);

            (diagnosticPolicy ?? new SpiceDiagnosticPolicy()).Apply(
                AllDiagnostics,
                out IReadOnlyList<SpiceDiagnostic> effectiveDiagnostics,
                out IReadOnlyList<SpiceDiagnostic> suppressedDiagnostics);
            Diagnostics = effectiveDiagnostics;
            SuppressedDiagnostics = suppressedDiagnostics;
            PolicySuccess = Success
                && !Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        }

        /// <summary>
        /// Gets a value indicating whether compilation produced a simulation-ready model without errors.
        /// </summary>
        public bool Success => Model != null;

        /// <summary>
        /// Gets a value indicating whether compilation succeeded and the effective diagnostics pass policy.
        /// </summary>
        public bool PolicySuccess { get; }

        /// <summary>
        /// Gets the parsed model before preprocessing, when parsing reached that point.
        /// </summary>
        public SpiceNetlist InputModel { get; }

        /// <summary>
        /// Gets the model after preprocessing, when preprocessing reached that point.
        /// </summary>
        public SpiceNetlist ExpandedModel { get; }

        /// <summary>
        /// Gets the translated model only when no error diagnostics remain; otherwise, null.
        /// </summary>
        public SpiceSharpModel Model { get; }

        /// <summary>
        /// Gets the partially translated model when the reader stage ran, including when errors were reported.
        /// Do not simulate this model unless <see cref="Success"/> is true.
        /// </summary>
        public SpiceSharpModel TranslatedModel { get; }

        /// <summary>
        /// Gets effective, non-suppressed diagnostics in pipeline order.
        /// Policy severity changes are reflected here.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets every raw diagnostic before suppression and policy severity changes.
        /// These diagnostics determine simulation safety and compatibility.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> AllDiagnostics { get; }

        /// <summary>
        /// Gets non-error diagnostics hidden from the effective diagnostic view by policy.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> SuppressedDiagnostics { get; }

        /// <summary>
        /// Gets external .INCLUDE and file-backed .LIB dependencies in discovery order.
        /// </summary>
        public IReadOnlyList<SpiceDependency> Dependencies { get; }

        /// <summary>
        /// Gets the aggregated simulation blockers and compatibility differences.
        /// </summary>
        public CompatibilityReport Compatibility { get; }

        /// <summary>
        /// Gets the dialect applied to both parser and reader stages.
        /// </summary>
        public SpiceDialect EffectiveDialect { get; }
    }
}
