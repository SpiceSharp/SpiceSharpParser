using System;
using SpiceSharpParser.Diagnostics;

namespace SpiceSharpParser
{
    /// <summary>
    /// Represents one external file reference encountered during preprocessing.
    /// A separate entry is retained for every directive occurrence.
    /// </summary>
    public sealed class SpiceDependency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceDependency"/> class.
        /// </summary>
        public SpiceDependency(
            SpiceDependencyKind kind,
            string requestedPath,
            string resolvedPath,
            SpiceDependencyStatus status,
            SourceSpan directiveSpan,
            string librarySection = null)
        {
            if (string.IsNullOrWhiteSpace(requestedPath))
            {
                throw new ArgumentException("A requested dependency path is required.", nameof(requestedPath));
            }

            Kind = kind;
            RequestedPath = requestedPath;
            ResolvedPath = resolvedPath;
            Status = status;
            DirectiveSpan = directiveSpan;
            LibrarySection = librarySection;
        }

        /// <summary>
        /// Gets the kind of directive that introduced the dependency.
        /// </summary>
        public SpiceDependencyKind Kind { get; }

        /// <summary>
        /// Gets the path as written in the directive after parsing quotes.
        /// </summary>
        public string RequestedPath { get; }

        /// <summary>
        /// Gets the path resolved against the containing file's directory or compilation working directory.
        /// </summary>
        public string ResolvedPath { get; }

        /// <summary>
        /// Gets the resolution outcome.
        /// </summary>
        public SpiceDependencyStatus Status { get; }

        /// <summary>
        /// Gets the span of the .INCLUDE or .LIB directive that introduced the dependency.
        /// </summary>
        public SourceSpan DirectiveSpan { get; }

        /// <summary>
        /// Gets the source file containing the dependency directive, when known.
        /// </summary>
        public string SourcePath => DirectiveSpan.FilePath;

        /// <summary>
        /// Gets the selected .LIB section, when one was specified.
        /// </summary>
        public string LibrarySection { get; }

        /// <summary>
        /// Gets a value indicating whether the dependency was successfully resolved and read.
        /// </summary>
        public bool IsResolved => Status == SpiceDependencyStatus.Resolved;
    }
}
