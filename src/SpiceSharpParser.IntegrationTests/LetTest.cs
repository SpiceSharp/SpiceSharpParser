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

        [Fact]
        public void ReferenceInLetTest()
        {
            var netlist = ParseNetlist(
                "Subcircuit test",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "R1 OUT 0 1",
                "\n",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=1 R2=1",
                "X1 input 1 resistor R={R1}",
                "X2 1 output resistor R={R2}",
                ".ENDS twoResistorsInSeries",
                "\n",
                ".OP",
                ".LET double_current {@X1.X1.R1[i] * 2}",
                ".LET triple_current_plus_1 {@X1.X1.R1[i] * 3 + 1}",
                ".SAVE V(OUT) double_current triple_current_plus_1",
                ".END");

            double[] export = RunOpSimulation(netlist, "V(OUT)", "double_current", "triple_current_plus_1");

            Assert.Equal(1, export[0]);
            Assert.Equal(2, export[1]);
            Assert.Equal(4, export[2]);
        }
    }
}
