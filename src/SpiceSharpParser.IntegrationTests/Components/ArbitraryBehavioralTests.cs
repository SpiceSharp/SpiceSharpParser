using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class ArbitraryBehavioralTests : BaseTests
    {
        [Fact]
        public void Test01()
        {
             var model = GetSpiceSharpModel(
                "ArbitraryBehavioral source",
                "B1 1 0 v = {10 * 10}",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "V(1,0)");
            Assert.Equal(100, export);
        }

        [Fact]
        public void Test02()
        {
            var model = GetSpiceSharpModel(
               "ArbitraryBehavioral source",
               "B1 1 0 i = {10 * 10}",
               "R1 1 0 10",
               ".SAVE V(1,0)",
               ".OP",
               ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "V(1,0)");
            Assert.Equal(-1000, export);
        }
    }
}
