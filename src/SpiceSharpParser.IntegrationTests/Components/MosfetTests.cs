using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class MosfetTests : BaseTests
    {
        [Fact]
        public void PmosLevel1()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos level = 1",
                ".END");

            Assert.NotNull(netlist);

            var model = netlist.Circuit["my-pmos"];
            Assert.True(model is Mosfet1Model);
        }

        [Fact]
        public void PmosLevel2()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos level = 2",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-pmos"];
            Assert.True(model is Mosfet2Model);
        }

        [Fact]
        public void PmosLevel3()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos level = 3",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-pmos"];
            Assert.True(model is Mosfet3Model);
        }

        [Fact]
        public void NmosLevel1()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos level = 1",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-nmos"];
            Assert.True(model is Mosfet1Model);
        }

        [Fact]
        public void NmosLevel2()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos level = 2",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-nmos"];
            Assert.True(model is Mosfet2Model);
        }

        [Fact]
        public void NmosLevel3()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos level = 3",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-nmos"];
            Assert.True(model is Mosfet3Model);
        }

        //
        [Fact]
        public void PmosLevel1Bracket()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos (level = 1)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-pmos"];
            Assert.True(model is Mosfet1Model);
        }

        [Fact]
        public void PmosLevel2Bracket()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos (level = 2)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-pmos"];
            Assert.True(model is Mosfet2Model);
        }

        [Fact]
        public void PmosLevel3Bracket()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-pmos",
                ".model my-pmos pmos(level = 3)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-pmos"];
            Assert.True(model is Mosfet3Model);
        }

        [Fact]
        public void NmosLevel1Bracket()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos(level = 1)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-nmos"];
            Assert.True(model is Mosfet1Model);
        }

        [Fact]
        public void NmosLevel2Bracket()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos(level = 2)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-nmos"];
            Assert.True(model is Mosfet2Model);
        }

        [Fact]
        public void NmosLevel3Bracket()
        {
            var netlist = GetSpiceSharpModel(
                "Mosfet circuit",
                "Md 0 1 2 3 my-nmos",
                ".model my-nmos nmos(level = 3)",
                ".END");

            Assert.NotNull(netlist);
            var model = netlist.Circuit["my-nmos"];
            Assert.True(model is Mosfet3Model);
        }
    }
}