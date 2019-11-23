using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class FormatTests : BaseTests
    {
        [Fact]
        public void MixedFormat()
        {
            var netlist = ParseNetlist(
                "ExpressionTests - CUSTOM FUNCTION + EXPRESSIONS",
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
    }
}