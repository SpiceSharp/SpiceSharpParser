using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class TempTests : BaseTests
    {
        [Fact]
        public void TempVariableWorks()
        {
            var model = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is={TEMP == 26 ? 2.52e-9 : 2.24e-9} N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".TEMP 26 27",
                ".END");

            Assert.Equal(2, model.Exports.Count);

            var export = RunSimulationsAndReturnExports(model);
            Assert.Equal(2, export.Count);
            Assert.True(EqualsWithTol(2.30935768424922E-09, (double)export[0]));
            Assert.True(EqualsWithTol(2.2407198249641E-09, (double)export[1]));
        }

        [Fact]
        public void TempVariableParamWorks()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "R1 1 0 {X}",
                "V1 1 0 10",
                ".OP",
                ".PARAM X=\"TEMP/2\"",
                ".SAVE i(V1)",
                ".TEMP 26 27",
                ".END");

            Assert.Equal(2, netlist.Exports.Count);

            var export = RunSimulationsAndReturnExports(netlist);
            
            Assert.Equal(2, export.Count);
            Assert.True(EqualsWithTol(-0.769230769230769, (double)export[0]));
            Assert.True(EqualsWithTol(-0.740740740740741, (double)export[1]));
        }

        [Fact]
        public void TempStatementMultiplySimulations()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
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
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
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

            Assert.True(EqualsWithTol(2.30935768424922E-09, export));
        }

        [Fact]
        public void WhenTempIs27EqualsToSpice()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS TEMP=27",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Assert.True(EqualsWithTol(2.5206849402909E-09, export));
        }

        [Fact]
        public void WhenTempIsNot27()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS TEMP=26",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Assert.True(EqualsWithTol(2.30935768424922E-09, export));
        }

        [Fact]
        public void WhenTNomIs27EqualsToSpice()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS Tnom=27",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Assert.True(EqualsWithTol(2.5206849402909E-09, export));
        }

        [Fact]
        public void WhenTNomIsNot27()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".OPTIONS Tnom=26",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Assert.True(EqualsWithTol(2.75136240873719E-09, export)); 
        }

        [Fact]
        public void NoOptionsEqualsToSpice()
        {
            var netlist = GetSpiceSharpModel(
                "Temp - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".END");

            var export = RunOpSimulation(netlist, "i(V1)");
            Assert.True(EqualsWithTol(2.5206849402909E-09, export));
        }
    }
}