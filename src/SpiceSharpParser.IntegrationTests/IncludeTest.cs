using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class IncludeTest : BaseTest
    {
        [Fact]
        public void SingleIncludeTest()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".include \"{modelFilePath}\"",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }
    }
}
