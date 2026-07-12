using System.Globalization;
using System.Threading;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Mappings
{
    public class BaseMapperTests
    {
        [Fact]
        public void CaseInsensitiveLookupDoesNotDependOnCurrentCulture()
        {
            var previousCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                var expected = new object();
                var mapper = new BaseMapper<object>();
                mapper.Map("I", expected);

                Assert.True(mapper.ContainsKey("i", false));
                Assert.Same(expected, mapper.GetValue("i", false));
                Assert.True(mapper.TryGetValue("i", false, out var actual));
                Assert.Same(expected, actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }
    }
}