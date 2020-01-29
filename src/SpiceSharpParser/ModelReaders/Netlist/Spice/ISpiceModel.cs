using System.Collections.Generic;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public interface ISpiceModel<out TCircuit, TSimulation>
    {
        /// <summary>
        /// Gets the circuit from the netlist.
        /// </summary>
        TCircuit Circuit { get; }

        /// <summary>
        /// Gets the list of simulation from the netlist.
        /// </summary>
        List<TSimulation> Simulations { get; }

        /// <summary>
        /// Gets the title of the netlist.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the list of comments from the netlist.
        /// </summary>
        List<string> Comments { get; }

        /// <summary>
        /// Gets the list of exports from the netlist.
        /// </summary>
        List<Export> Exports { get; }

        /// <summary>
        /// Gets the list of generated X-Y plots.
        /// </summary>
        List<XyPlot> XyPlots { get; }

        /// <summary>
        /// Gets the Monte Carlo Analysis results.
        /// </summary>
        MonteCarloResult MonteCarloResult { get; }

        /// <summary>
        /// Gets the simulation configuration.
        /// </summary>
        SimulationConfiguration SimulationConfiguration { get; }

        /// <summary>
        /// Gets the list of generated prints.
        /// </summary>
        List<Print> Prints { get; }

        /// <summary>
        /// Gets or sets the used random seed.
        /// </summary>
        int? Seed { get; set; }

        ValidationEntryCollection ValidationResult { get; }
    }
}
