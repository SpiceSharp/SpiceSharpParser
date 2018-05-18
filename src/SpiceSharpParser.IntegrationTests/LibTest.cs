using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class LibTest : BaseTest
    {
        [Fact]
        public void BasicTest()
        {
            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l1");

            File.WriteAllText(l1Path, ".lib a\n*comment\n.endl\n");

            var netlist = ParseNetlistInWorkingDirectory(
                Directory.GetCurrentDirectory(),
                "Lib - Basic",
                ".lib l1 a",
                ".END");

            Assert.Single(netlist.Comments);
        }

        [Fact]
        public void LibWithInclude()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l1");
            File.WriteAllText(l1Path, ".lib a\n.include diodes.mod\n.endl\n");

            var netlist = ParseNetlist(
                "Lib - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".lib l1 a",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }

        [Fact]
        public void LibWithLib()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            string basicPath = Path.Combine(Directory.GetCurrentDirectory(), "basic");
            File.WriteAllText(basicPath, ".lib diodes\n.include diodes.mod\n.endl\n");

            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l1");
            File.WriteAllText(l1Path, ".lib a\n.lib basic diodes\n.endl\n");

            var netlist = ParseNetlist(
                "Lib - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".lib l1 a",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }
    }
}
