using System.Linq;

namespace SpiceSharpParser.Common
{
    /// <summary>
    /// Extension methods for <see cref="ISimulationWithEvents"/>.
    /// </summary>
    public static class SimulationExtensions
    {
        /// <summary>
        /// Runs the simulation on the given circuit, invokes all event handlers,
        /// and blocks until the simulation completes.
        /// </summary>
        /// <param name="simulation">The simulation to run.</param>
        /// <param name="circuit">The circuit to simulate.</param>
        public static void Execute(this ISimulationWithEvents simulation, SpiceSharp.Circuit circuit)
        {
            simulation.InvokeEvents(simulation.Run(circuit, -1)).ToArray();
        }
    }
}
