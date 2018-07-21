using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// Simulation context.
    /// </summary>
    public class SimulationContext
    {
        /// <summary>
        /// Gets or sets context's simulation.
        /// </summary>
        public Simulation Simulation { get; set; }

        /// <summary>
        /// Gets or sets the evaluator for the context.
        /// </summary>
        public IEvaluator Evaluator { get; set; }
    }
}
