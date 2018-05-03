using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class SaveTest : BaseTest
    {
        [Fact]
        public void LetTest()
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
        public void VoltageInternalNodeInSubcircuitTest()
        {
            var netlist = ParseNetlist(
                "Subcircuit test",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT) V(X1.1)",
                ".END");

            double[] export = RunOpSimulation(netlist, "V(OUT)", "V(X1.1)");

            Assert.Equal(1, export[0]);
            Assert.Equal(3, export[1]);
        }

        [Fact]
        public void CurrentInSubcircuitTest()
        {
            var netlist = ParseNetlist(
                "Subcircuit test",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT) I(X1.R1)",
                ".END");

            double[] export = RunOpSimulation(netlist, "V(OUT)", "I(X1.R1)");

            Assert.Equal(1, export[0]);
            Assert.Equal(1, export[1]);
        }

        [Fact]
        public void VoltagePublicNodeInSubcircuitTest()
        {
            var netlist = ParseNetlist(
                "Subcircuit test",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT) V(X1.input)",
                ".END");

            double[] export = RunOpSimulation(netlist, "V(OUT)", "V(X1.input)");

            Assert.Equal(1, export[0]);
            Assert.Equal(4, export[1]);
        }

        [Fact]
        public void VoltagePublicNodeInNestedSubcircuitTest()
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
                ".SAVE V(OUT) V(X1.X1.input)",
                ".END");

            double[] export = RunOpSimulation(netlist, "V(OUT)", "V(X1.X1.input)");

            Assert.Equal(1, export[0]);
            Assert.Equal(4, export[1]);
        }

        [Fact]
        public void CurrentInNestedSubcircuitTest()
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
                ".SAVE V(OUT) I(X1.X1.R1)",
                ".END");

            double[] export = RunOpSimulation(netlist, "V(OUT)", "I(X1.X1.R1)");

            Assert.Equal(1, export[0]);
            Assert.Equal(1, export[1]);
        }

        [Fact]
        public void VoltageExtremeNodeInNestedSubcircuitTest()
        {
            var netlist = ParseNetlist(
                "Subcircuit test",
                "V1 IN 0 4.0",
                "X1 IN 0 fourResistorsInSeries R1=1 R2=1 R3=1 R4=1",
                "\n",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=1 R2=1",
                "X1 input 1 resistor R={R1}",
                "X2 1 output resistor R={R2}",
                ".ENDS twoResistorsInSeries",
                ".SUBCKT fourResistorsInSeries input output params: R1=1 R2=1 R3=1 R4=1",
                "X1 input 1 twoResistorsInSeries R1={R1} R2={R2}",
                "X2 1 output twoResistorsInSeries R1={R3} R2={R4}",
                ".ENDS fourResistorsInSeries",
                "\n",
                ".OP",
                ".SAVE V(IN) V(X1.input) V(X1.output) V(X1.1) V(X1.X1.input) V(X1.X1.output) V(X1.X1.1)",
                ".END");

            double[] exports = RunOpSimulation(netlist, "V(IN)", "V(X1.input)", "V(X1.output)", "V(X1.1)", "V(X1.X1.input)", "V(X1.X1.output)", "V(X1.X1.1)");

            // Verify
            this.Compare(exports, new double[] { 4, 4, 0, 2, 4, 2, 3 });
        }

        [Fact]
        public void CurrentInExtremeNestedSubcircuitTest()
        {
            var netlist = ParseNetlist(
                "Subcircuit test",
                "V1 IN 0 4.0",
                "X1 IN 0 fourResistorsInSeries R1=1 R2=1 R3=1 R4=1",
                "\n",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=1 R2=1",
                "X1 input 1 resistor R={R1}",
                "X2 1 output resistor R={R2}",
                ".ENDS twoResistorsInSeries",
                ".SUBCKT fourResistorsInSeries input output params: R1=1 R2=1 R3=1 R4=1",
                "X1 input 1 twoResistorsInSeries R1={R1} R2={R2}",
                "X2 1 output twoResistorsInSeries R1={R3} R2={R4}",
                ".ENDS fourResistorsInSeries",
                "\n",
                ".OP",
                ".SAVE V(IN) I(X1.X1.X1.R1)",
                ".END");

            double[] exports = RunOpSimulation(netlist, "V(IN)", "I(X1.X1.X1.R1)");

            // Verify
            this.Compare(exports, new double[] { 4, 1 });
        }

    }
}
