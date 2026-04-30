using SpiceSharpParser.Lexers.Netlist.Spice;
using System.Globalization;
using System.Threading;
using Xunit;

namespace SpiceSharpParser.Tests.Lexers.Spice
{
    public class SpiceTokenExtensionsTests
    {
        [Fact]
        public void When_CurrentCultureIsTurkish_Expect_CaseInsensitiveComparisonIsOrdinal()
        {
            var previousCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");

                var token = new SpiceToken(SpiceTokenType.WORD, "distribution");

                Assert.True(token.Equal("DISTRIBUTION", false));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }
    }
}
