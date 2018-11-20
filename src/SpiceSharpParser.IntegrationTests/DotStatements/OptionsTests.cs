using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class OptionsTests : BaseTests
    {
        [Fact]
        public void DynamicResistorsTest()
        {
            var netlist = ParseNetlist(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(V(in), 1e-3)}",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".OPTIONS dynamic-resistors",
                ".END");

            var exports = RunDCSimulation(netlist, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / Math.Max(1e-3, (sweep - 1e-3));
            EqualsWithTol(exports, reference);
        }
    }
}
