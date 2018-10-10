using System;
using Xunit;

namespace SpiceSharpParser.PerformanceTests
{
    public class ExpressionTests : BaseTests
    {
        [Fact]
        public void ModelIsUsingExpression()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs={sin(0) + 0 + 0 + sin(0) + sin(0) + sin(0) + 0.568} N={sin(0) + 0 + 0 + sin(0) + sin(0) + sin(0) + 1.752} Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 1e-6",
                ".SAVE i(V1)",
                ".END");

            RunDCSimulation(netlist, "i(V1)");
        }
    }
}
