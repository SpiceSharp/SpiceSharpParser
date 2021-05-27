using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class ConnectTests : BaseTests
    {
        [Fact]
        public void SingleConnectionTwoNodes()
        {
            var model = GetSpiceSharpModel(
                "ConnectTests - Diode circuit",
                "D1 D1_OUT 0 default",
                "V1 OUT 0 0",
                ".model default D",
                ".CONNECT D1_OUT OUT",
                ".DC V1 -1 1 0.5",
                ".SAVE i(V1)",
                ".END");

            var export = RunDCSimulation(model, "i(V1)");

            // Get reference
            double[] references =
            {
               1.00999976741763E-12,
               5.0999813934108E-13,
               0,
               -2.48698224986992E-06,
               -618.507827392572
            };

            Assert.True(EqualsWithTol(export, references));
        }
    }
}