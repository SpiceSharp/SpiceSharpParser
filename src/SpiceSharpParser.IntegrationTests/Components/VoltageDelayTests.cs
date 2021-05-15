using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class VoltageDelayTests : BaseTests
    {
        [Fact]
        public void VoltageDelay_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage delay",
                "V1 1 0 SINE(0 5 50 0 0 90)",
                "BVDELAY1 2 0 1 0 1e-2",
                "R1 1 0 10",
                "R2 2 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasError);
            Assert.False(model.ValidationResult.HasWarning);

            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }
    }
}