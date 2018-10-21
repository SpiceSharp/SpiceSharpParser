using SpiceSharp.Simulations;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class NodeSetTests : BaseTests
    {
        [Fact]
        public void Verify()
        {
            var netlist = ParseNetlist(
                "NodeSet - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, ((BaseSimulation)netlist.Simulations[0]).Configurations.Get<BaseConfiguration>().Nodesets["OUT"]);
        }
    }
}
