using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class TempTest : BaseTest
    {
        [Fact]
        public void WhenTempIs27EqualsToSpice3f5()
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
        public void WhenTNomIs27EqualsToSpice3f5()
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
            Compare(export, 2.75136240873719E-09); // value not from Spice3f5
        }

        [Fact]
        public void NoOptionsEqualsToSpice3f5()
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
