using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class NetlistAdder : INetlistAdder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetlistAdder"/> class.
        /// </summary>
        /// <param name="netlist">A netlist</param>
        public NetlistAdder(Netlist netlist)
        {
            Netlist = netlist;
        }

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
    }
}
