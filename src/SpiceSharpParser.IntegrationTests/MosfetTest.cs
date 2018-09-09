using SpiceSharp.Components;
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

            var model = netlist.Circuit.Objects["my-pmos"];
            Assert.True(model is Mosfet1Model);
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
            var model = netlist.Circuit.Objects["my-pmos"];
            Assert.True(model is Mosfet2Model);
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
            var model = netlist.Circuit.Objects["my-pmos"];
            Assert.True(model is Mosfet3Model);
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
            var model = netlist.Circuit.Objects["my-nmos"];
            Assert.True(model is Mosfet1Model);
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
            var model = netlist.Circuit.Objects["my-nmos"];
            Assert.True(model is Mosfet2Model);
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
            var model = netlist.Circuit.Objects["my-nmos"];
            Assert.True(model is Mosfet3Model);
        }


        //
        [Fact]
        public void PmosLevel1BracketTest()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos (level = 1)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit.Objects["my-pmos"];
            Assert.True(model is Mosfet1Model);
        }

        [Fact]
        public void PmosLevel2BracketTest()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos (level = 2)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit.Objects["my-pmos"];
            Assert.True(model is Mosfet2Model);
        }

        [Fact]
        public void PmosLevel3BracketTest()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos(level = 3)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit.Objects["my-pmos"];
            Assert.True(model is Mosfet3Model);
        }

        [Fact]
        public void NmosLevel1BracketTest()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos(level = 1)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit.Objects["my-nmos"];
            Assert.True(model is Mosfet1Model);
        }

        [Fact]
        public void NmosLevel2BracketTest()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos(level = 2)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit.Objects["my-nmos"];
            Assert.True(model is Mosfet2Model);
        }

        [Fact]
        public void NmosLevel3BracketTest()
        {
            var netlist = ParseNetlist(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos(level = 3)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit.Objects["my-nmos"];
            Assert.True(model is Mosfet3Model);
        }
    }
}
