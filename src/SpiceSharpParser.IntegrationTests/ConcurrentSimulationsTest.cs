using SpiceSharp.Simulations;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class ConcurrentSimulationsTest : BaseTest
    {
        [Fact]
        public void OPTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST 1N914(N) 0.1 10.0 0.01",
                ".STEP X 1 10 1",
                ".END");

            Parallel.ForEach<Simulation>(netlist.Simulations, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, simulation => simulation.Run(netlist.Circuit));
        }        
    }
}
