using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Configures the diagnostic view used by CI, editors, and other compilation consumers.
    /// Policy never changes whether a translated model is safe to simulate.
    /// </summary>
    public sealed class SpiceDiagnosticPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceDiagnosticPolicy"/> class.
        /// </summary>
        public SpiceDiagnosticPolicy()
        {
            SuppressedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            SeverityOverrides = new Dictionary<string, DiagnosticSeverity>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets a value indicating whether effective warning diagnostics fail policy evaluation.
        /// </summary>
        public bool WarningsAsErrors { get; set; }

        /// <summary>
        /// Gets diagnostic codes hidden from the effective diagnostic view.
        /// Error diagnostics cannot be suppressed.
        /// </summary>
        public ISet<string> SuppressedCodes { get; }

        /// <summary>
        /// Gets severity overrides keyed by diagnostic code.
        /// Raw errors cannot be downgraded; overrides only affect the policy view.
        /// </summary>
        public IDictionary<string, DiagnosticSeverity> SeverityOverrides { get; }

        internal void Apply(
            IEnumerable<SpiceDiagnostic> diagnostics,
            out IReadOnlyList<SpiceDiagnostic> effectiveDiagnostics,
            out IReadOnlyList<SpiceDiagnostic> suppressedDiagnostics)
        {
            var effective = new List<SpiceDiagnostic>();
            var suppressed = new List<SpiceDiagnostic>();

            foreach (SpiceDiagnostic diagnostic in diagnostics)
            {
                if (diagnostic.Severity != DiagnosticSeverity.Error
                    && SuppressedCodes.Contains(diagnostic.Code))
                {
                    suppressed.Add(diagnostic);
                    continue;
                }

                DiagnosticSeverity severity = GetEffectiveSeverity(diagnostic);
                effective.Add(severity == diagnostic.Severity
                    ? diagnostic
                    : CopyWithSeverity(diagnostic, severity));
            }

            effectiveDiagnostics = effective.AsReadOnly();
            suppressedDiagnostics = suppressed.AsReadOnly();
        }

        internal void Validate()
        {
            if (SuppressedCodes.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Suppressed diagnostic codes cannot be null or whitespace.", nameof(SuppressedCodes));
            }

            foreach (KeyValuePair<string, DiagnosticSeverity> item in SeverityOverrides)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    throw new ArgumentException("Severity override codes cannot be null or whitespace.", nameof(SeverityOverrides));
                }

                if (!Enum.IsDefined(typeof(DiagnosticSeverity), item.Value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(SeverityOverrides),
                        item.Value,
                        $"Unknown diagnostic severity for '{item.Key}'.");
                }
            }
        }

        private DiagnosticSeverity GetEffectiveSeverity(SpiceDiagnostic diagnostic)
        {
            DiagnosticSeverity severity = diagnostic.Severity;
            if (SeverityOverrides.TryGetValue(diagnostic.Code, out DiagnosticSeverity severityOverride))
            {
                severity = severityOverride;
            }

            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                return DiagnosticSeverity.Error;
            }

            return WarningsAsErrors && severity == DiagnosticSeverity.Warning
                ? DiagnosticSeverity.Error
                : severity;
        }

        private static SpiceDiagnostic CopyWithSeverity(
            SpiceDiagnostic diagnostic,
            DiagnosticSeverity severity)
        {
            return new SpiceDiagnostic(
                diagnostic.Code,
                severity,
                diagnostic.Stage,
                diagnostic.Message,
                diagnostic.Span,
                diagnostic.RelatedLocations,
                diagnostic.Construct,
                diagnostic.SuggestedFix,
                diagnostic.CompatibilityClass,
                diagnostic.HelpLink,
                diagnostic.IncludeStack);
        }
    }
}
