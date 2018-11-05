using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class LosslessTransmissionLineTests : BaseTests
    {
        [Fact]
        public void IsAbleToParse()
        {
            var netlist = ParseNetlist(
               "Lossless transmission line parse test circuit",
               "T1 1 2 3 4 z0 = 230 Td = 120ns",
               "T2 1 2 3 4 z0=250 f=2.6MEG",
               ".END");

            Assert.NotNull(netlist);
        }
    }
}
