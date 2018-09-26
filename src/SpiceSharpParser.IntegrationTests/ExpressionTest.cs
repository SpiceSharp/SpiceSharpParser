using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class ExpressionTest : BaseTest
    {
        [Fact]
        public void TableVoltageTest()
        {
            var result = ParseNetlist(
                "Table test circuit",
                "V1 0 1 1.5",
                "V2 1 2 {X}",
                "R1 1 0 100",
                ".OP",
                ".SAVE V(1,2)",
                ".PARAM X={table(V(0,1), 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Equal(1, result.Exports.Count);
            Assert.Equal(1, result.Simulations.Count);

            var export = RunOpSimulation(result, "V(1, 2)");

            Assert.Equal(15, export);
        }

        [Fact]
        public void TableMinTest()
        {
            var result = ParseNetlist(
                "Table test circuit",
                "V1 0 1 150",
                "V2 1 2 1.5",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM R={table(0, 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Equal(1, result.Exports.Count);
            Assert.Equal(1, result.Simulations.Count);

            var export = RunOpSimulation(result, "i(R1)");

            Assert.Equal(-150 / 10, export);
        }

        [Fact]
        public void TableAvgTest()
        {
            var result = ParseNetlist(
                "Table test circuit",
                "V1 0 1 150",
                "V2 1 2 1.5",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM R={table(1.5, 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Equal(1, result.Exports.Count);
            Assert.Equal(1, result.Simulations.Count);

            var export = RunOpSimulation(result, "i(R1)");

            Assert.Equal(-150 / 15, export);
        }

        [Fact]
        public void TableMaxTest()
        {
            var result = ParseNetlist(
                "Table test circuit",
                "V1 0 1 150",
                "V2 1 2 1.5",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM R={table(4, 1, 10, 2, 20, 3, 30)}",
                ".END");

            Assert.Equal(1, result.Exports.Count);
            Assert.Equal(1, result.Simulations.Count);

            var export = RunOpSimulation(result, "i(R1)");

            Assert.Equal(-150 / 30, export);
        }

        [Fact]
        public void ExpressionsMixed()
        {
            var netlist = ParseNetlist(
                "PARAM user function test",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".model 1N914 D(Is=2.52e-9 Rs={0.568} N='1.752' Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE VOUT_db VOUT_db2 V(OUT)",
                ".PARAM decibels_plus_param(value,x)='log10(value)*2+x' add(x,y)={x+y}",
                ".LET VOUT_db 'add(decibels_plus_param(V(OUT),1), -0.5)'",
                ".LET VOUT_db2 {add(decibels_plus_param(V(OUT),1), -0.2)}",
                ".END");

            double[] export = RunOpSimulation(netlist, new string[] { "VOUT_db", "VOUT_db2", "V(OUT)" });

            Assert.Equal(2.5, export[0]);
            Assert.Equal(2.8, export[1]);
            Assert.Equal(10, export[2]);
        }

        [Fact]
        public void RereferenceMixed()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP (voltage test)",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {@R1[resistance]*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".END");

            var exports = RunSimulationsAndReturnExports(result);
        }
    }
}
