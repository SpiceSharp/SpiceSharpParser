using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps
{
    public interface IParameterSweepUpdater
    {
        /// <summary>
        /// Sets sweep parameters for the simulation.
        /// </summary>
        /// <param name="simulation">Simulation to set.</param>
        /// <param name="context">Reading context.</param>
        /// <param name="parameterValues">Parameter values.</param>
        void Update(Simulation simulation, IReadingContext context, List<KeyValuePair<Parameter, double>> parameterValues);
    }
}