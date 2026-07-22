using System;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Describes a source location related to a diagnostic's primary span.
    /// </summary>
    public sealed class DiagnosticRelatedLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticRelatedLocation"/> class.
        /// </summary>
        /// <param name="span">The related source span.</param>
        /// <param name="message">A description of the relationship.</param>
        public DiagnosticRelatedLocation(SourceSpan span, string message)
        {
            Span = span;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Gets the related source span.
        /// </summary>
        public SourceSpan Span { get; }

        /// <summary>
        /// Gets a description of the relationship.
        /// </summary>
        public string Message { get; }
    }
}
