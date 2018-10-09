using System.Collections.Generic;
using SpiceSharpParser.Common.StringComparers;

namespace SpiceSharpParser.Common
{
    public static class StringComparerProvider
    {
        private static IEqualityComparer<string> caseSensitiveComparer = new CaseSensitiveComparer();
        private static IEqualityComparer<string> caseInsensitiveComparer = new CaseInsensitiveComparer();

        public static IEqualityComparer<string> Get(bool caseSensitive)
        {
            return caseSensitive ? caseSensitiveComparer : caseInsensitiveComparer;
        }
    }
}
