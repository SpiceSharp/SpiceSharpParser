using System.Collections.Generic;
using SpiceSharpParser.Common.StringComparers;

namespace SpiceSharpParser.Common
{
    public static class StringComparerFactory
    {
        public static IEqualityComparer<string> Create(bool caseSensitive)
        {
            return caseSensitive ? (IEqualityComparer<string>)new CaseSensitiveComparer() : new CaseInsensitiveStrongComparer();
        }
    }
}
