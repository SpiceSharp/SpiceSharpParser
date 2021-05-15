using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class FuncTests : BaseTests
    {
        [Fact]
        public void FuncBasic()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction(4)}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction(x) = {x * x + 1}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
        }

        [Fact]
        public void FuncOverloading()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction(4)}",
                "R2 OUT 0 {somefunction(4, 1)}",
                ".OP",
                ".SAVE V(OUT) @R1[i] @R2[i]",
                ".FUNC somefunction(x) = {x * x + 1}",
                ".FUNC somefunction(x, y) = {x * x + y}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]", "@R2[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
            Assert.Equal(10.0 / 17.0, export[2]);
        }

        [Fact]
        public void FuncOverloadingOverrides()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction(4)}",
                "R2 OUT 0 {somefunction(4, 1)}",
                ".OP",
                ".SAVE V(OUT) @R1[i] @R2[i]",
                ".FUNC somefunction(x) = {x * x + 1}",
                ".FUNC somefunction(x, y) = {x * x + y + 2}",
                ".FUNC somefunction(x, y) = {x * x + y}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]", "@R2[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
            Assert.Equal(10.0 / 17.0, export[2]);
        }

        [Fact]
        public void FuncMuliple()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {otherfunction(somefunction(4))}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction(x) = {x * x} otherfunction(x) = {x + 5}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 21.0, export[1]);
        }

        [Fact]
        public void FuncWithoutEq()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test without '='",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction(4)}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction(x) {x * x + 1}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
        }

        [Fact]
        public void FuncWithoutArguments()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction()}",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction() = {17}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 17.0, export[1]);
        }

        [Fact]
        public void FuncWithVoltageFunctionWithArgument()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 1 0 10.0",
                "R1 1 0 {somefunction(1)}",
                ".OP",
                ".SAVE V(1) @R1[i]",
                ".FUNC somefunction(x) = {V(x) + 10.0}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(1)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 20.0, export[1]);
        }

        [Fact]
        public void FuncWithVoltageFunction()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 {somefunction()}",
                "V2 1 0 17",
                "R2 1 0 100",
                ".OP",
                ".SAVE V(OUT) @R1[i]",
                ".FUNC somefunction() = {V(1) + 10.0}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "V(OUT)", "@R1[i]" });

            Assert.Equal(10.0, export[0]);
            Assert.Equal(10.0 / 27.0, export[1]);
        }

        [Fact]
        public void FuncValue()
        {
            var model = GetSpiceSharpModel(
                "FUNC user function test",
                "V1 OUT 0 10.0",
                "R1 OUT 0 1",
                "R2 2 0 10",
                "ESource1 2 0 VALUE = { somefunction(4) * 2 }",
                ".OP",
                ".SAVE I(R2)",
                ".PARAM abc = 1",
                ".FUNC somefunction(x) = {V(OUT) + x + abc}",
                ".END");

            double[] export = RunOpSimulation(model, new string[] { "I(R2)" });

            Assert.Equal(3, export[0]);
        }
    }
}