using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.StringComparers
{
    public class CaseSensitiveComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(string x, string y)
        {
            // Do case insensitive equality checking on the identifiers
            return StringComparer.Ordinal.Equals(x?.ToString(), y?.ToString());
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(string obj)
        {
            // Make sure the hash code is case insensitive
            return obj.ToString().GetHashCode();
        }
    }
}
