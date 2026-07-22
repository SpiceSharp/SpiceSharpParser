using System;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Identifies a position in a SPICE source document.
    /// Lines and columns are one-based. A zero value means that part of the position is unknown.
    /// </summary>
    public readonly struct SourcePosition : IEquatable<SourcePosition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePosition"/> struct.
        /// </summary>
        /// <param name="line">The one-based line, or zero when unknown.</param>
        /// <param name="column">The one-based column, or zero when unknown.</param>
        public SourcePosition(int line, int column)
        {
            if (line < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            if (column < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            if (line == 0 && column != 0)
            {
                throw new ArgumentException("A column cannot be specified when the line is unknown.", nameof(column));
            }

            Line = line;
            Column = column;
        }

        /// <summary>
        /// Gets an unknown source position.
        /// </summary>
        public static SourcePosition Unknown => default;

        /// <summary>
        /// Gets the one-based line, or zero when unknown.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the one-based column, or zero when unknown.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets a value indicating whether the line is known.
        /// </summary>
        public bool IsKnown => Line > 0;

        /// <summary>
        /// Gets a value indicating whether the column is known.
        /// </summary>
        public bool HasColumn => Column > 0;

        /// <summary>
        /// Determines whether two positions are equal.
        /// </summary>
        public static bool operator ==(SourcePosition left, SourcePosition right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two positions are different.
        /// </summary>
        public static bool operator !=(SourcePosition left, SourcePosition right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(SourcePosition other)
        {
            return Line == other.Line && Column == other.Column;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SourcePosition other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Line * 397) ^ Column;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!IsKnown)
            {
                return "unknown";
            }

            return HasColumn ? $"{Line}:{Column}" : Line.ToString();
        }
    }
}
