using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Expressions
{
    public class FuncTests : BaseTests
    {
        [Fact]
        public void FuncBasic()
        {
            var netlist = ParseNetlist(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction(4)}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction(x) = {x * x + 1}",
                ".END");

            double[] export = RunOpSimulation(netlist, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
        }

        [Fact]
        public void FuncMuliple()
        {
            var netlist = ParseNetlist(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {otherfunction(somefunction(4))}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction(x) = {x * x} otherfunction(x) = {x + 5}",
                ".END");

            double[] export = RunOpSimulation(netlist, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 21.0, export[1]);
        }

        [Fact]
        public void FuncWithoutEq()
        {
            var netlist = ParseNetlist(
                "FUNC user function test without '='",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction(4)}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction(x) {x * x + 1}",
                ".END");

            double[] export = RunOpSimulation(netlist, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
        }

        [Fact]
        public void FuncWithoutArguments()
        {
            var netlist = ParseNetlist(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction()}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction() = {17}",
                ".END");

            double[] export = RunOpSimulation(netlist, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
        }
    }
}
