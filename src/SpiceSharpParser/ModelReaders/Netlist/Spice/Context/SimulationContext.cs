using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// TODO: Add comments.
    /// </summary>
    public class SimulationContext
    {
        public Simulation Simulation { get; set; }

        public IEvaluator Evaluator { get; set; }
    }
}
