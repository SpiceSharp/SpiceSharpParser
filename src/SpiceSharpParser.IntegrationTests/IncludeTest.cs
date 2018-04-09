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

        [Fact]
        public void SingleIncludeWorkingDirectoryFileTest()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            var netlist = ParseNetlistInWorkingDirectory(
                Directory.GetCurrentDirectory(),
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".include \"diodes.mod\"",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }

        [Fact]
        public void SingleIncludeSubDirectoryLinuxStyleFileTest()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "common", "diodes.mod");

            string subdirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "common");
            if (!Directory.Exists(subdirectoryPath))
            {
                Directory.CreateDirectory(subdirectoryPath);
            }

            File.WriteAllText(modelFilePath, modelFileContent);

            var netlist = ParseNetlistInWorkingDirectory(
                Directory.GetCurrentDirectory(),
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".include \"common/diodes.mod\"",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }

        [Fact]
        public void SingleIncludeSubDirectoryWindowsStyleFileTest()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "common", "diodes.mod");

            string subdirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "common");
            if (!Directory.Exists(subdirectoryPath))
            {
                Directory.CreateDirectory(subdirectoryPath);
            }

            File.WriteAllText(modelFilePath, modelFileContent);

            var netlist = ParseNetlistInWorkingDirectory(
                Directory.GetCurrentDirectory(),
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".include \"common\\diodes.mod\"",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }

        [Fact]
        public void NestedIncludeSubDirectoryFileTest()
        {
            string model1N914FileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string model1N914FilePath = Path.Combine(Directory.GetCurrentDirectory(), "common", "1N914.mod");
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "common", "diodes.mod");
            string modelFileContent = ".include \"1N914.mod\"\n";

            string subdirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "common");
            if (!Directory.Exists(subdirectoryPath))
            {
                Directory.CreateDirectory(subdirectoryPath);
            }

            File.WriteAllText(modelFilePath, modelFileContent);
            File.WriteAllText(model1N914FilePath, model1N914FileContent);

            var netlist = ParseNetlistInWorkingDirectory(
                Directory.GetCurrentDirectory(),
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".include \"common\\diodes.mod\"",
                ".END");

            RunDCSimulation(netlist, "V(OUT)");
            Assert.Equal(14, netlist.Simulations[0].Nodes.NodeSets["OUT"]);
        }
    }
}
