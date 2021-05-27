using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class VSwitchTests : BaseTests
    {
        [Fact]
        public void PartialOnTest()
        {
            var model = GetSpiceSharpModel(
                "VSwitch test",
                "S1 1 0 2 0 smodel",
                "V1 2 0 0.5",
                "V2 1 0 10",
                ".model smodel VSWITCH (roff=1000000 ron=10 voff = 0 von = 1)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(model, "I(V2)");
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(-0.00316228, export));
        }

        [Fact]
        public void PartialOnSecondTest()
        {
            var model = GetSpiceSharpModel(
                "VSwitch test",
                "S1 1 0 2 0 smodel",
                "V1 2 0 11",
                "V2 1 0 10",
                ".model smodel VSWITCH (roff=5000 ron=10 voff = 10 von = 130)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(model, "I(V2)");
            
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(-0.00200258, export));
        }

        [Fact]
        public void OnTest()
        {
            var model = GetSpiceSharpModel(
                "VSwitch test",
                "S1 1 0 2 0 smodel",
                "V1 2 0 2",
                "V2 1 0 10",
                ".model smodel VSWITCH (roff=1000000 ron=10 voff = 0 von = 1)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(model, "I(V2)");
            Assert.NotNull(model);
            Assert.Equal(-1, export);
        }

        [Fact]
        public void OnMoreTest()
        {
            var model = GetSpiceSharpModel(
                "VSwitch test",
                "S1 1 0 2 0 smodel",
                "V1 2 0 2000",
                "V2 1 0 10",
                ".model smodel VSWITCH (roff=1000000 ron=10 voff = 0 von = 1)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(model, "I(V2)");
            Assert.NotNull(model);
            Assert.Equal(-1, export);
        }

        [Fact]
        public void OffTest()
        {
            var model = GetSpiceSharpModel(
                "VSwitch test",
                "S1 1 0 2 0 smodel",
                "V1 2 0 0",
                "V2 1 0 10",
                ".model smodel VSWITCH (roff=1000000 ron=10 voff = 0 von = 1)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(model, "I(V2)");
            
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(-10.0 / 1000000.0, export));
        }

        [Fact]
        public void OffMoreTest()
        {
            var model = GetSpiceSharpModel(
                "VSwitch test",
                "S1 1 0 2 0 smodel",
                "V1 2 0 -1",
                "V2 1 0 10",
                ".model smodel VSWITCH (roff=1000000 ron=10 voff = 0 von = 1)",
                ".OP",
                ".SAVE I(V2)",
                ".END");

            var export = RunOpSimulation(model, "I(V2)");
            
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(-10.0 / 1000000.0, export));
        }
    }
}