using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpiceSharpParser.Diagnostics;

namespace SpiceSharpParser
{
    /// <summary>
    /// Summarizes compilation blockers and known compatibility differences.
    /// </summary>
    public sealed class CompatibilityReport
    {
        private const string UnknownGroup = "<unknown>";

        internal CompatibilityReport(IEnumerable<SpiceDiagnostic> diagnostics, bool canSimulate)
        {
            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            List<SpiceDiagnostic> issues = diagnostics.ToList();
            CanSimulate = canSimulate;
            IssueCount = issues.Count;
            Blockers = Snapshot(issues.Where(IsBlocker));
            Warnings = Snapshot(issues.Where(issue => issue.Severity == DiagnosticSeverity.Warning));
            Unsupported = Snapshot(issues.Where(IsUnsupported));
            Approximated = Snapshot(issues.Where(issue => issue.CompatibilityClass == CompatibilityClass.ParserShim));
            Ignored = Snapshot(issues.Where(issue => issue.CompatibilityClass == CompatibilityClass.RecognizedNoOp));
            Divergences = Snapshot(issues.Where(issue => issue.CompatibilityClass == CompatibilityClass.NumericDivergence));
            Unclassified = Snapshot(issues.Where(issue => !issue.CompatibilityClass.HasValue));
            IssuesByConstruct = CountBy(issues, issue => issue.Construct);
            IssuesByFile = CountBy(issues, issue => issue.Span.FilePath);
            IssuesByCode = CountBy(issues, issue => issue.Code);
        }

        /// <summary>
        /// Gets a value indicating whether the compiled model is safe to simulate.
        /// </summary>
        public bool CanSimulate { get; }

        /// <summary>
        /// Gets a value indicating whether no blocking, unsupported, approximated, or divergent behavior was reported.
        /// Ignored metadata does not make a compilation incompatible.
        /// </summary>
        public bool IsFullyCompatible => BlockerCount == 0
            && Unsupported.Count == 0
            && Approximated.Count == 0
            && Divergences.Count == 0;

        /// <summary>
        /// Gets the total number of diagnostics considered by this report.
        /// </summary>
        public int IssueCount { get; }

        /// <summary>
        /// Gets the number of error diagnostics that block simulation readiness.
        /// </summary>
        public int BlockerCount => Blockers.Count;

        /// <summary>
        /// Gets the number of warning diagnostics.
        /// </summary>
        public int WarningCount => Warnings.Count;

        /// <summary>
        /// Gets all error diagnostics that block simulation readiness.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Blockers { get; }

        /// <summary>
        /// Gets all warning diagnostics.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Warnings { get; }

        /// <summary>
        /// Gets known unsupported, engine-required, parse-only, and compatibility-gap diagnostics.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Unsupported { get; }

        /// <summary>
        /// Gets diagnostics for constructs lowered or approximated by compatibility shims.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Approximated { get; }

        /// <summary>
        /// Gets diagnostics for recognized metadata that is intentionally ignored.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Ignored { get; }

        /// <summary>
        /// Gets diagnostics for documented numeric divergences.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Divergences { get; }

        /// <summary>
        /// Gets diagnostics that are not compatibility classifications, such as malformed syntax or lint findings.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Unclassified { get; }

        /// <summary>
        /// Gets deterministic diagnostic counts grouped by construct. Unknown constructs use the &lt;unknown&gt; key.
        /// </summary>
        public IReadOnlyDictionary<string, int> IssuesByConstruct { get; }

        /// <summary>
        /// Gets deterministic diagnostic counts grouped by source file. Unknown files use the &lt;unknown&gt; key.
        /// </summary>
        public IReadOnlyDictionary<string, int> IssuesByFile { get; }

        /// <summary>
        /// Gets deterministic diagnostic counts grouped by stable diagnostic code.
        /// </summary>
        public IReadOnlyDictionary<string, int> IssuesByCode { get; }

        private static IReadOnlyList<SpiceDiagnostic> Snapshot(IEnumerable<SpiceDiagnostic> diagnostics)
        {
            return diagnostics.ToList().AsReadOnly();
        }

        private static bool IsBlocker(SpiceDiagnostic diagnostic)
        {
            return diagnostic.Severity == DiagnosticSeverity.Error;
        }

        private static bool IsUnsupported(SpiceDiagnostic diagnostic)
        {
            return diagnostic.CompatibilityClass == CompatibilityClass.TargetedDiagnostic
                || diagnostic.CompatibilityClass == CompatibilityClass.SyntaxAuditGap
                || diagnostic.CompatibilityClass == CompatibilityClass.EngineRequired
                || diagnostic.CompatibilityClass == CompatibilityClass.ParseOnly;
        }

        private static IReadOnlyDictionary<string, int> CountBy(
            IEnumerable<SpiceDiagnostic> diagnostics,
            Func<SpiceDiagnostic, string> keySelector)
        {
            var result = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (SpiceDiagnostic diagnostic in diagnostics)
            {
                string key = keySelector(diagnostic);
                if (string.IsNullOrWhiteSpace(key))
                {
                    key = UnknownGroup;
                }

                result[key] = result.TryGetValue(key, out int count) ? count + 1 : 1;
            }

            return new ReadOnlyDictionary<string, int>(result);
        }
    }
}
