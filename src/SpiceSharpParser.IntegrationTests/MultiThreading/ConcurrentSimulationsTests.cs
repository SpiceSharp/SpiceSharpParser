using System.Threading.Tasks;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.MultiThreading
{
    public class ConcurrentSimulationsTests : BaseTests
    {
        [Fact]
        public void OPSweep()
        {
            var netlist = GetSpiceSharpModel(
                "ConcurrentSimulations - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST 1N914(N) 0.1 10.0 0.01",
                ".STEP X 1 10 1",
                ".END");

            var exception = Record.Exception(() => Parallel.ForEach(netlist.Simulations, new ParallelOptions { MaxDegreeOfParallelism = 1 }, simulation => simulation.Run(netlist.Circuit)));
            
            Assert.Null(exception);
        }

        [Fact]
        public void OPMonteCarlo()
        {
            var result = GetSpiceSharpModel(
                "ConcurrentSimulations - Monte Carlo Analysis - OP",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {@R1[i]}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".END");

            var exception = Record.Exception(() => Parallel.ForEach(result.Simulations, new ParallelOptions { MaxDegreeOfParallelism = 8 }, simulation => simulation.Run(result.Circuit)));

            Assert.Null(exception);
        }
    }
}