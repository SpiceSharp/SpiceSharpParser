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
        /// <param name="result">A SPICE model reader result.</param>
        public ResultService(SpiceModel<Circuit, Simulation> result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Gets simulation configuration.
        /// </summary>
        public SimulationConfiguration SimulationConfiguration => Result.SimulationConfiguration;

        /// <summary>
        /// Gets all simulations.
        /// </summary>
        public IEnumerable<Simulation> Simulations => Result.Simulations;

        /// <summary>
        /// Gets all exports.
        /// </summary>
        public IEnumerable<Export> Exports => Result.Exports;

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public Circuit Circuit => Result.Circuit;

        /// <summary>
        /// Gets the Monte Carlo result.
        /// </summary>
        public MonteCarloResult MonteCarlo => Result.MonteCarloResult;

        /// <summary>
        /// Gets or sets used random seed.
        /// </summary>
        public int? Seed
        {
            get => Result.Seed;
            set => Result.Seed = value;
        }

        public SpiceNetlistValidationResult Validation => Result.ValidationResult;

        /// <summary>
        /// Gets the result where things are added.
        /// </summary>
        private SpiceModel<Circuit, Simulation> Result { get; }

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

            Result.Comments.Add(statement.Text);
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

            Result.Exports.Add(export);
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

            Result.XyPlots.Add(plot);
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

            Result.Prints.Add(print);
        }

        /// <summary>
        /// Adds entity to netlist.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddEntity(IEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Result.Circuit.Add(entity);
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

            Result.Simulations.Add(simulation);
        }

        /// <summary>
        /// Finds the object in the result.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>
        /// True if found.
        /// </returns>
        public bool FindObject(string objectId, out IEntity entity)
        {
            if (objectId == null)
            {
                throw new ArgumentNullException(nameof(objectId));
            }

            return Result.Circuit.TryGetEntity(objectId, out entity);
        }
    }
}