using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class NoiseTests : BaseTests
    {
        [Fact]
        public void NoException()
        {
            var model = GetSpiceSharpModel(
                "Noise",
                "V1 in 0 10.0",
                "R1 in out 10",
                "C1 out 0 20",
                ".NOISE V(out) V1 DEC 10 1 1e9",
                ".END");

            RunSimulations(model);

            Assert.False(model.ValidationResult.HasError);
            Assert.False(model.ValidationResult.HasWarning);
        }

        [Fact]
        public void NoExceptionVectorVoltage()
        {
            var model = GetSpiceSharpModel(
                "Noise",
                "V1 in 0 10.0",
                "R1 in out 10",
                "C1 out 0 20",
                ".NOISE V(out, 0) V1 dec 10 1 1e9",
                ".END");

            RunSimulations(model);

            Assert.False(model.ValidationResult.HasError);
            Assert.False(model.ValidationResult.HasWarning);
        }
    }
}
