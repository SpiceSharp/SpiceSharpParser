using System.Collections.Generic;
using SpiceSharpParser.Common.StringComparers;

namespace SpiceSharpParser.Common
{
    public static class StringComparerFactory
    {
        public static IEqualityComparer<string> Get(bool caseSensitive)
        {
            return caseSensitive ? (IEqualityComparer<string>)new CaseSensitiveComparer() : new CaseInsensitiveComparer();
        }
    }
}
