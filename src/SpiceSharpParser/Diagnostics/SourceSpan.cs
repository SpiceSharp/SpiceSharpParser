using System;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Identifies a range in a SPICE source document.
    /// The start is inclusive and the end is exclusive.
    /// </summary>
    public readonly struct SourceSpan : IEquatable<SourceSpan>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceSpan"/> struct.
        /// </summary>
        /// <param name="filePath">The source path or source name, when known.</param>
        /// <param name="start">The inclusive start position.</param>
        /// <param name="end">The exclusive end position.</param>
        public SourceSpan(string filePath, SourcePosition start, SourcePosition end)
        {
            if (start.IsKnown && end.IsKnown && IsBefore(end, start))
            {
                throw new ArgumentException("The end position cannot precede the start position.", nameof(end));
            }

            FilePath = filePath;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets an unknown source span.
        /// </summary>
        public static SourceSpan Unknown => default;

        /// <summary>
        /// Gets the source path or source name, when known.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the inclusive start position.
        /// </summary>
        public SourcePosition Start { get; }

        /// <summary>
        /// Gets the exclusive end position.
        /// </summary>
        public SourcePosition End { get; }

        /// <summary>
        /// Gets a value indicating whether at least a source line is known.
        /// </summary>
        public bool IsKnown => Start.IsKnown;

        /// <summary>
        /// Determines whether two spans are equal.
        /// </summary>
        public static bool operator ==(SourceSpan left, SourceSpan right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two spans are different.
        /// </summary>
        public static bool operator !=(SourceSpan left, SourceSpan right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Creates a source span from the parser's legacy line information.
        /// </summary>
        /// <param name="lineInfo">The legacy line information.</param>
        /// <returns>A one-based source span, or <see cref="Unknown"/>.</returns>
        public static SourceSpan FromLineInfo(SpiceLineInfo lineInfo)
        {
            if (lineInfo == null)
            {
                return Unknown;
            }

            if (lineInfo.LineNumber <= 0)
            {
                return new SourceSpan(lineInfo.FileName, SourcePosition.Unknown, SourcePosition.Unknown);
            }

            var startColumn = lineInfo.StartColumnIndex >= 0 ? lineInfo.StartColumnIndex + 1 : 0;
            var endColumn = lineInfo.EndColumnIndex >= 0 ? lineInfo.EndColumnIndex + 1 : 0;

            return new SourceSpan(
                lineInfo.FileName,
                new SourcePosition(lineInfo.LineNumber, startColumn),
                new SourcePosition(lineInfo.LineNumber, endColumn));
        }

        /// <inheritdoc/>
        public bool Equals(SourceSpan other)
        {
            return string.Equals(FilePath, other.FilePath, StringComparison.Ordinal)
                && Start.Equals(other.Start)
                && End.Equals(other.End);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SourceSpan other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FilePath != null ? StringComparer.Ordinal.GetHashCode(FilePath) : 0;
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var location = IsKnown ? Start.ToString() : "unknown";
            return string.IsNullOrEmpty(FilePath) ? location : $"{FilePath}({location})";
        }

        private static bool IsBefore(SourcePosition left, SourcePosition right)
        {
            if (left.Line != right.Line)
            {
                return left.Line < right.Line;
            }

            return left.HasColumn && right.HasColumn && left.Column < right.Column;
        }
    }
}
