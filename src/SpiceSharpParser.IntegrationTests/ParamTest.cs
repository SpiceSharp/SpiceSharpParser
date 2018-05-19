using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class ParamTest : BaseTest
    {
        [Fact]
        public void ParamAdvancedTest()
        {
            var netlist = ParseNetlist(
                "PARAM user function test",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".SAVE VOUT_db V(OUT)",
                ".PARAM decibels_plus_param(value,x)={log10(value)*2+x} add(x,y)={x+y}",
                ".LET VOUT_db {add(decibels_plus_param(V(OUT),1), -0.5)}",
                ".END");

            double[] export = RunOpSimulation(netlist, new string[] { "VOUT_db", "V(OUT)" });

            Assert.Equal(export[0], 2.5);
            Assert.Equal(export[1], 10);
        }
    }
}
