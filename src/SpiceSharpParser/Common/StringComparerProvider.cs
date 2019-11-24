using SpiceSharpParser.Common.StringComparers;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public static class StringComparerProvider
    {
        private static readonly EqualityComparer<string> CaseSensitiveComparer = new CaseSensitiveComparer();
        private static readonly EqualityComparer<string> CaseInsensitiveComparer = new CaseInsensitiveComparer();

        public static EqualityComparer<string> Get(bool caseSensitive)
        {
            return caseSensitive ? CaseSensitiveComparer : CaseInsensitiveComparer;
        }
    }
}