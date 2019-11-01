using System.Collections.Generic;
using SpiceSharpParser.Common.StringComparers;

namespace SpiceSharpParser.Common
{
    public static class StringComparerProvider
    {
        private static readonly EqualityComparer<string> caseSensitiveComparer = new CaseSensitiveComparer();
        private static readonly EqualityComparer<string> caseInsensitiveComparer = new CaseInsensitiveComparer();

        public static EqualityComparer<string> Get(bool caseSensitive)
        {
            return caseSensitive ? caseSensitiveComparer : caseInsensitiveComparer;
        }
    }
}
