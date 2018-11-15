using SpiceSharpParser.Parsers.Netlist;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class SubcircuitTests : BaseTests
    {
        [Fact]
        public void SubcircuitEnding()
        {
            var netlist = ParseNetlist(
                "Subcircuit - SubcircuitEndingTest",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT treeResistorsInSeries input output params: R1=10 R2=100 R3=1000",
                "R1 input 1 {R1}",
                "R2 1 2 {R2}",
                "R3 2 output {R3}",
                ".ENDS",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { 1.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void SubcircuitParams()
        {
            var netlist = ParseNetlist(
                "Subcircuit - SubcircuitEndingTest",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries PARAMS: R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT treeResistorsInSeries input output params: R1=10 R2=100 R3=1000",
                "R1 input 1 {R1}",
                "R2 1 2 {R2}",
                "R3 2 output {R3}",
                ".ENDS",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { 1.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void SingleSubcircuitWithParams()
        {
            var netlist = ParseNetlist(
                "Subcircuit - SingleSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { 1.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void SingleSubcircuitWithoutParamsKeyword()
        {
            var netlist = ParseNetlist(
                "Subcircuit - SingleSubcircuitWithoutParamsKeyword",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { 1.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void SingleSubcircuitWithDefaultParams()
        {
            var netlist = ParseNetlist(
                "Subcircuit - SingleSubcircuitWithDefaultParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { (1.0 / ( 10.0 + 20.0 + 1.0)) * 4.0};

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void ComplexSubcircuitWithParams()
        {
            var netlist = ParseNetlist(
                "Subcircuit - ComplexSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R2",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { (1.0 / (10.0 + 20.0 + 1.0)) * 4.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void ComplexContainedSubcircuitWithParams()
        {
            var netlist = ParseNetlist(
                "Subcircuit - ComplexContainedSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R2",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { (1.0 / (10.0 + 20.0 + 1.0)) * 4.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void ComplexContainedSubcircuitWithParamsAndParamControl()
        {
            var netlist = ParseNetlist(
                "Subcircuit - ComplexContainedSubcircuitWithParamsAndParamControl",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R3",
                ".param R3={R2*1}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { (1.0 / (10.0 + 20.0 + 1.0)) * 4.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void SubcircuitWithWrongEnding()
        {
            try
            {
                var netlist = ParseNetlist(
                    "Subcircuit - ComplexContainedSubcircuitWithParams",
                    "V1 IN 0 4.0",
                    "X1 IN OUT resistor",
                    "RX OUT 0 1",
                    ".SUBCKT resistor input output params: R=1",
                    "R1 input output {R}",
                    ".ENDS resistor2",
                    ".OP",
                    ".SAVE V(OUT)",
                    ".END");

                Assert.True(false, "There should be exception");
            }
            catch (ParseException)
            {
            }
        }
    }
}
