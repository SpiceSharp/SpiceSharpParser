using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.StringComparers
{
    public class CaseSensitiveComparer : EqualityComparer<string>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <see cref="string"/> to compare.</param>
        /// <param name="y">The second object of type <see cref="string"/> to compare.</param>
        public override bool Equals(string x, string y)
        {
            // Do case insensitive equality checking on the identifiers
            return StringComparer.Ordinal.Equals(x, y);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode(string obj)
        {
            // Make sure the hash code is case sensitive.
            return obj.GetHashCode();
        }
    }
}