using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Represents a structured diagnostic produced while processing a SPICE netlist.
    /// </summary>
    public sealed class SpiceDiagnostic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceDiagnostic"/> class.
        /// </summary>
        public SpiceDiagnostic(
            string code,
            DiagnosticSeverity severity,
            DiagnosticStage stage,
            string message,
            SourceSpan span = default,
            IEnumerable<DiagnosticRelatedLocation> relatedLocations = null,
            string construct = null,
            string suggestedFix = null,
            CompatibilityClass? compatibilityClass = null,
            Uri helpLink = null,
            IEnumerable<SourceSpan> includeStack = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("A diagnostic code is required.", nameof(code));
            }

            Code = code;
            Severity = severity;
            Stage = stage;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Span = span;
            RelatedLocations = (relatedLocations ?? Enumerable.Empty<DiagnosticRelatedLocation>()).ToList().AsReadOnly();
            IncludeStack = (includeStack ?? Enumerable.Empty<SourceSpan>()).ToList().AsReadOnly();
            Construct = construct;
            SuggestedFix = suggestedFix;
            CompatibilityClass = compatibilityClass;
            HelpLink = helpLink ?? SpiceDiagnosticCodes.GetHelpLink(code);
        }

        /// <summary>
        /// Gets the stable diagnostic code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the diagnostic severity.
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the compilation stage that produced the diagnostic.
        /// </summary>
        public DiagnosticStage Stage { get; }

        /// <summary>
        /// Gets the primary source span.
        /// </summary>
        public SourceSpan Span { get; }

        /// <summary>
        /// Gets source locations related to the primary diagnostic.
        /// </summary>
        public IReadOnlyList<DiagnosticRelatedLocation> RelatedLocations { get; }

        /// <summary>
        /// Gets include or library directive spans from the root source to the file containing this diagnostic.
        /// </summary>
        public IReadOnlyList<SourceSpan> IncludeStack { get; }

        /// <summary>
        /// Gets the recognized SPICE construct, when available.
        /// </summary>
        public string Construct { get; }

        /// <summary>
        /// Gets the diagnostic message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets a suggested corrective action, when available.
        /// </summary>
        public string SuggestedFix { get; }

        /// <summary>
        /// Gets the compatibility classification, when applicable.
        /// </summary>
        public CompatibilityClass? CompatibilityClass { get; }

        /// <summary>
        /// Gets a link to additional diagnostic documentation, when available.
        /// </summary>
        public Uri HelpLink { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Code}: {Span}: {Message}";
        }
    }
}
