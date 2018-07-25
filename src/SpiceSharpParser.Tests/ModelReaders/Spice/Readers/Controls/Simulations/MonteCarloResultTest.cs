using SpiceSharp.Simulations;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class MonteCarloResultTest
    {
        [Fact]
        public void CollectMinMaxTest()
        {
            var monteCarloResult = new MonteCarloResult();

            var op1 = new OP("op1");
            var op2 = new OP("op2");
            monteCarloResult.Collect(op1, 1);
            monteCarloResult.Collect(op1, 2);

            Assert.Equal(1, monteCarloResult.Min[op1]);
            Assert.Equal(2, monteCarloResult.Max[op1]);

            monteCarloResult.Collect(op2, -1);
            monteCarloResult.Collect(op2, 4);

            Assert.Equal(-1, monteCarloResult.Min[op2]);
            Assert.Equal(4, monteCarloResult.Max[op2]);
        }

        [Fact]
        public void GetPlotMaxTest()
        {
            var monteCarloResult = new MonteCarloResult();

            var sim = new OP("op1");

            monteCarloResult.Collect(sim, -1);
            monteCarloResult.Collect(sim, 4);
            monteCarloResult.Function = "MAX";

            var plot = monteCarloResult.GetPlot(10);
            Assert.Single(plot.Bins.Keys);
            Assert.Equal(4, plot.Bins[1].Value);
        }

        [Fact]
        public void GetPlotMaxMoreBinsTest()
        {
            var monteCarloResult = new MonteCarloResult();

            var sim = new OP("op1");
            var sim2 = new OP("op2");

            monteCarloResult.Collect(sim, -1);
            monteCarloResult.Collect(sim, 4);
            monteCarloResult.Collect(sim2, 0);
            monteCarloResult.Collect(sim2, 1);
            monteCarloResult.Function = "MAX";

            var plot = monteCarloResult.GetPlot(10);
            Assert.Equal(10, plot.Bins.Keys.Count);
        }

        [Fact]
        public void GetPlotMinTest()
        {
            var monteCarloResult = new MonteCarloResult();

            var sim = new OP("op1");

            monteCarloResult.Collect(sim, -1);
            monteCarloResult.Collect(sim, 4);
            monteCarloResult.Function = "MIN";

            var plot = monteCarloResult.GetPlot(10);
            Assert.Single(plot.Bins.Keys);
            Assert.Equal(-1, plot.Bins[1].Value);
        }

        [Fact]
        public void GetPlotMinMoreBinsTest()
        {
            var monteCarloResult = new MonteCarloResult();

            var sim = new OP("op1");
            var sim2 = new OP("op2");

            monteCarloResult.Collect(sim, -1);
            monteCarloResult.Collect(sim, 4);
            monteCarloResult.Collect(sim2, 0);
            monteCarloResult.Collect(sim2, 1);

            monteCarloResult.Function = "MIN";

            var plot = monteCarloResult.GetPlot(10);
            Assert.Equal(10, plot.Bins.Keys.Count);
        }
    }
}
