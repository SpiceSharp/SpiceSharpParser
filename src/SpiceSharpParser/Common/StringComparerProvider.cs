using System.Collections.Generic;
using SpiceSharpParser.Common.StringComparers;

namespace SpiceSharpParser.Common
{
    public static class StringComparerProvider
    {
        private static readonly IEqualityComparer<string> caseSensitiveComparer = new CaseSensitiveComparer();
        private static readonly IEqualityComparer<string> caseInsensitiveComparer = new CaseInsensitiveComparer();

        public static IEqualityComparer<string> Get(bool caseSensitive)
        {
            return caseSensitive ? caseSensitiveComparer : caseInsensitiveComparer;
        }
    }
}
