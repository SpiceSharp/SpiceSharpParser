using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class ResultService : IResultService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultService"/> class.
        /// </summary>
        /// <param name="model">A SPICE model reader result.</param>
        public ResultService(SpiceModel<Circuit, Simulation> model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Gets simulation configuration.
        /// </summary>
        public SimulationConfiguration SimulationConfiguration => Model.SimulationConfiguration;

        /// <summary>
        /// Gets all simulations.
        /// </summary>
        public IEnumerable<Simulation> Simulations => Model.Simulations;

        /// <summary>
        /// Gets all exports.
        /// </summary>
        public IEnumerable<Export> Exports => Model.Exports;

        /// <summary>
        /// Gets the Monte Carlo result.
        /// </summary>
        public MonteCarloResult MonteCarlo => Model.MonteCarloResult;

        /// <summary>
        /// Gets or sets used random seed.
        /// </summary>
        public int? Seed
        {
            get => Model.Seed;
            set => Model.Seed = value;
        }

        public SpiceNetlistValidationResult Validation => Model.ValidationResult;

        /// <summary>
        /// Gets the result where things are added.
        /// </summary>
        private SpiceModel<Circuit, Simulation> Model { get; }

        /// <summary>
        /// Adds comment.
        /// </summary>
        /// <param name="statement">Comment to add.</param>
        public void AddComment(CommentLine statement)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            Model.Comments.Add(statement.Text);
        }

        /// <summary>
        /// Adds export to netlist.
        /// </summary>
        /// <param name="export">Export to add.</param>
        public void AddExport(Export export)
        {
            if (export == null)
            {
                throw new ArgumentNullException(nameof(export));
            }

            Model.Exports.Add(export);
        }

        /// <summary>
        /// Adds plot to netlist.
        /// </summary>
        /// <param name="plot">Plot to add.</param>
        public void AddPlot(XyPlot plot)
        {
            if (plot == null)
            {
                throw new ArgumentNullException(nameof(plot));
            }

            Model.XyPlots.Add(plot);
        }

        /// <summary>
        /// Adds print to netlist.
        /// </summary>
        /// <param name="print">Print to add.</param>
        public void AddPrint(Print print)
        {
            if (print == null)
            {
                throw new ArgumentNullException(nameof(print));
            }

            Model.Prints.Add(print);
        }

        /// <summary>
        /// Adds simulation to netlist.
        /// </summary>
        /// <param name="simulation">Simulation to add.</param>
        public void AddSimulation(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            Model.Simulations.Add(simulation);
        }
    }
}