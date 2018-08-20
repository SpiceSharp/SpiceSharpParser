using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class MosfetTest : BaseTest
    {
        [Fact]
        public void PmosLevel1Test()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos level = 1",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void PmosLevel2Test()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos level = 2",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void PmosLevel3Test()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos level = 3",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void NmosLevel1Test()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos level = 1",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void NmosLevel2Test()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos level = 2",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void NmosLevel3Test()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos level = 3",
                ".END");

            Assert.NotNull(netlist);
        }
    }
}
