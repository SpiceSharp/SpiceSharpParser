using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public class ResultService : IResultService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultService"/> class.
        /// </summary>
        /// <param name="netlist">A netlist</param>
        public ResultService(Netlist netlist)
        {
            Netlist = netlist;
        }

        /// <summary>
        /// Gets simulation configuration
        /// </summary>
        public SimulationConfiguration SimulationConfiguration => new SimulationConfiguration();

        /// <summary>
        /// Gets all simulations
        /// </summary>
        public IEnumerable<Simulation> Simulations => Netlist.Simulations;

        /// <summary>
        /// Gets th netlist where things are added
        /// </summary>
        private Netlist Netlist { get; }

        /// <summary>
        /// Adds warning
        /// </summary>
        /// <param name="warning">Warning to add</param>
        public void AddWarning(string warning)
        {
            Netlist.Warnings.Add(warning);
        }

        /// <summary>
        /// Adds comment
        /// </summary>
        /// <param name="statement">Comment to add</param>
        public void AddComment(CommentLine statement)
        {
            Netlist.Comments.Add(statement.Text);
        }

        /// <summary>
        /// Adds export to netlist
        /// </summary>
        /// <param name="export">Export to add</param>
        public void AddExport(Export export)
        {
            Netlist.Exports.Add(export);
        }

        /// <summary>
        /// Adds plot to netlist
        /// </summary>
        /// <param name="plot">Plot to add</param>
        public void AddPlot(Plot plot)
        {
            Netlist.Plots.Add(plot);
        }

        /// <summary>
        /// Adds entity to netlist
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public void AddEntity(Entity entity)
        {
            Netlist.Circuit.Objects.Add(entity);
        }

        /// <summary>
        /// Adds simulation to netlist
        /// </summary>
        /// <param name="simulation">Simulation to add</param>
        public void AddSimulation(BaseSimulation simulation)
        {
            Netlist.Simulations.Add(simulation);
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
            return Netlist.Circuit.Objects.TryGetEntity(objectName, out entity);
        }
    }
}
