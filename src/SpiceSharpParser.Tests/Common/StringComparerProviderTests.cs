using System.Collections.Generic;
using Xunit;
using StringComparerProvider = global::SpiceSharpParser.Common.StringComparerProvider;

namespace SpiceSharpParser.Tests.Common
{
    public class StringComparerProviderTests
    {
        [Fact]
        public void CaseInsensitiveComparerUsesEqualHashesForEqualUnicodeStrings()
        {
            var values = new Dictionary<string, int>(StringComparerProvider.Get(false))
            {
                ["Σ"] = 1,
            };

            Assert.Equal(1, values["ς"]);
        }
    }
}