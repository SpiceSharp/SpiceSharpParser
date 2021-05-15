using SpiceSharp.Simulations;
using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class LibTests : BaseTests
    {
        [Fact]
        public void Basic()
        {
            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l0");

            File.WriteAllText(l1Path, ".lib a\n*comment\n.endl\n");

            var model = GetSpiceSharpModelWithWorkingDirectoryParameter(
                Directory.GetCurrentDirectory(),
                "Lib - Basic",
                ".lib l0 a",
                ".END");

            Assert.Single(model.Comments);
        }

        [Fact]
        public void BasicMultipleEntries()
        {
            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l0");

            File.WriteAllText(l1Path, ".lib a\n*comment\n.endl\n.lib b\n*comment2\n.endl\n");

            var model = GetSpiceSharpModelWithWorkingDirectoryParameter(
                Directory.GetCurrentDirectory(),
                "Lib - Basic",
                ".lib l0 b",
                ".END");

            Assert.Single(model.Comments);
            Assert.Equal("*comment2", model.Comments[0]);
        }

        [Fact]
        public void BasicOneArgument()
        {
            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "lb");

            File.WriteAllText(l1Path, "*comment\n");

            var model = GetSpiceSharpModelWithWorkingDirectoryParameter(
                Directory.GetCurrentDirectory(),
                "Lib - Basic",
                ".lib lb",
                ".END");

            Assert.Single(model.Comments);
        }

        [Fact]
        public void LibWithInclude()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodeslib.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l1");
            File.WriteAllText(l1Path, ".lib a\n.include diodeslib.mod\n.endl\n");

            var model = GetSpiceSharpModel(
                "Lib - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".lib l1 a",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(model, "V(OUT)"));
            Assert.Null(exception);
        }

        [Fact]
        public void LibOneArgumentWithInclude()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes4_.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l12");
            File.WriteAllText(l1Path, ".include diodes4_.mod\n");

            var model = GetSpiceSharpModel(
                "Lib - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".lib l12",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(model, "V(OUT)"));
            Assert.Null(exception);
        }

        [Fact]
        public void LibWithLib()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes4.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            string basicPath = Path.Combine(Directory.GetCurrentDirectory(), "basic");
            File.WriteAllText(basicPath, ".lib diodes\n.include diodes4.mod\n.endl\n");

            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l1");
            File.WriteAllText(l1Path, ".lib a\n.lib basic diodes\n.endl\n");

            var model = GetSpiceSharpModel(
                "Lib - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".lib l1 a",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(model, "V(OUT)"));
            Assert.Null(exception);
        }

        [Fact]
        public void LibOneArgumentWithLib()
        {
            string modelFileContent = ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)\n";
            string modelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "diodes.mod");
            File.WriteAllText(modelFilePath, modelFileContent);

            string basicPath = Path.Combine(Directory.GetCurrentDirectory(), "basic");
            File.WriteAllText(basicPath, ".lib diodes\n.include diodes.mod\n.endl\n");

            string l1Path = Path.Combine(Directory.GetCurrentDirectory(), "l1");
            File.WriteAllText(l1Path, ".lib basic diodes\n");

            var model = GetSpiceSharpModel(
                "Lib - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".DC V1 -1 1 10e-3",
                ".SAVE V(OUT)",
                ".NODESET V(OUT)={x+1}",
                ".param x = 13",
                $".lib l1",
                ".END");

            var exception = Record.Exception(() => RunDCSimulation(model, "V(OUT)"));
            Assert.Null(exception);
        }
    }
}