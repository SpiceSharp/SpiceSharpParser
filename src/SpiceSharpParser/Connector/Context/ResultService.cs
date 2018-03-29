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
        /// <param name="netlist">A netlist</param>
        public ResultService(ConnectorResult netlist)
        {
            Result = netlist;
        }

        /// <summary>
        /// Gets simulation configuration
        /// </summary>
        public SimulationConfiguration SimulationConfiguration => new SimulationConfiguration();

        /// <summary>
        /// Gets all simulations
        /// </summary>
        public IEnumerable<Simulation> Simulations => Result.Simulations;

        /// <summary>
        /// Gets the result where things are added
        /// </summary>
        private ConnectorResult Result { get; }

        /// <summary>
        /// Adds warning
        /// </summary>
        /// <param name="warning">Warning to add</param>
        public void AddWarning(string warning)
        {
            Result.Warnings.Add(warning);
        }

        /// <summary>
        /// Adds comment
        /// </summary>
        /// <param name="statement">Comment to add</param>
        public void AddComment(CommentLine statement)
        {
            Result.Comments.Add(statement.Text);
        }

        /// <summary>
        /// Adds export to netlist
        /// </summary>
        /// <param name="export">Export to add</param>
        public void AddExport(Export export)
        {
            Result.Exports.Add(export);
        }

        /// <summary>
        /// Adds plot to netlist
        /// </summary>
        /// <param name="plot">Plot to add</param>
        public void AddPlot(Plot plot)
        {
            Result.Plots.Add(plot);
        }

        /// <summary>
        /// Adds entity to netlist
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public void AddEntity(Entity entity)
        {
            Result.Circuit.Objects.Add(entity);
        }

        /// <summary>
        /// Adds simulation to netlist
        /// </summary>
        /// <param name="simulation">Simulation to add</param>
        public void AddSimulation(BaseSimulation simulation)
        {
            Result.Simulations.Add(simulation);
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
            return Result.Circuit.Objects.TryGetEntity(objectName, out entity);
        }
    }
}
