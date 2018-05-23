using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class TempTest : BaseTest
    {
        [Fact]
        public void TempVariableWorks()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is={TEMP == 26 ? 2.52e-9 : 2.24e-9} N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".TEMP 26 27",
                ".END");

            Assert.Equal(2, netlist.Exports.Count);

            var export = RunSimulations(netlist);
            Assert.Equal(2, export.Count);
            Compare((double)export[0], 2.30935768424922E-09);
            Compare((double)export[1], 2.2407198249641E-09);
        }

        [Fact]
        public void TempStatementMultiplySimulations()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS TEMP=27",
                ".TEMP 26 27",
                ".END");

            Assert.Equal(2, netlist.Simulations.Count);
        }

        [Fact]
        public void TempStatementOverridesOptions()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS TEMP=27",
                ".TEMP 26",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Compare(export, 2.30935768424922E-09);
        }

        [Fact]
        public void WhenTempIs27EqualsToSpice()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS TEMP=27",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Compare(export, 2.5206849402909E-09);
        }

        [Fact]
        public void WhenTempIsNot27()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS TEMP=26",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Compare(export, 2.30935768424922E-09);
        }

        [Fact]
        public void WhenTNomIs27EqualsToSpice()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS Tnom=27",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Compare(export, 2.5206849402909E-09);
        }

        [Fact]
        public void WhenTNomIsNot27()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS Tnom=26",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Compare(export, 2.75136240873719E-09); // value not from Spice
        }

        [Fact]
        public void NoOptionsEqualsToSpice()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Compare(export, 2.5206849402909E-09);
        }
    }
}
