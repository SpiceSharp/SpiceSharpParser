using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class LetTest : BaseTest
    {
        [Fact]
        public void LetBasicTest()
        {
            var netlist = ParseNetlist(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".SAVE VX avg",
                ".LET VX {log10(V(OUT))*2}",
                ".LET avg {0.5 * (V(OUT) + V(IN))}",
                ".END");

            double[] export = RunOpSimulation(netlist, "VX", "avg");

            Assert.Equal(2, export[0]);
            Assert.Equal(10, export[1]);
        }
    }
}
