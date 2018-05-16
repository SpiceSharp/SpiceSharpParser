using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Connector.Processors.Controls.Plots;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Context
{
    public class ResultService : IResultService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultService"/> class.
        /// </summary>
        /// <param name="model">A Spice# model</param>
        public ResultService(SpiceSharpModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets simulation configuration.
        /// </summary>
        public SimulationConfiguration SimulationConfiguration { get; set; } = new SimulationConfiguration();

        /// <summary>
        /// Gets all simulations
        /// </summary>
        public IEnumerable<Simulation> Simulations => Model.Simulations;

        /// <summary>
        /// Gets the result where things are added
        /// </summary>
        private SpiceSharpModel Model { get; }

        /// <summary>
        /// Adds warning
        /// </summary>
        /// <param name="warning">Warning to add</param>
        public void AddWarning(string warning)
        {
            Model.Warnings.Add(warning);
        }

        /// <summary>
        /// Adds comment
        /// </summary>
        /// <param name="statement">Comment to add</param>
        public void AddComment(CommentLine statement)
        {
            Model.Comments.Add(statement.Text);
        }

        /// <summary>
        /// Adds export to netlist
        /// </summary>
        /// <param name="export">Export to add</param>
        public void AddExport(Export export)
        {
            Model.Exports.Add(export);
        }

        /// <summary>
        /// Adds plot to netlist
        /// </summary>
        /// <param name="plot">Plot to add</param>
        public void AddPlot(Plot plot)
        {
            Model.Plots.Add(plot);
        }

        /// <summary>
        /// Adds entity to netlist
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public void AddEntity(Entity entity)
        {
            Model.Circuit.Objects.Add(entity);
        }

        /// <summary>
        /// Adds simulation to netlist
        /// </summary>
        /// <param name="simulation">Simulation to add</param>
        public void AddSimulation(BaseSimulation simulation)
        {
            Model.Simulations.Add(simulation);
        }

        /// <summary>
        /// Sets the initial voltage
        /// </summary>
        /// <param name="nodeName">The node name</param>
        /// <param name="initialVoltage">The initial voltage</param>
        public void SetInitialVoltageCondition(string nodeName, double initialVoltage)
        {
            foreach (var simulation in Simulations)
            {
                simulation.Nodes.InitialConditions[nodeName] = initialVoltage;
            }
        }

        /// <summary>
        /// Finds the object
        /// </summary>
        /// <param name="objectName">The object name</param>
        /// <param name="entity">The found entity</param>
        /// <returns>
        /// True if found
        /// </returns>
        public bool FindObject(string objectName, out Entity entity)
        {
            return Model.Circuit.Objects.TryGetEntity(objectName, out entity);
        }
    }
}
