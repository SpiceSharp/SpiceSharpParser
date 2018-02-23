using System.Collections.Generic;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector
{
    /// <summary>
    /// A SpiceSharp netlist
    /// </summary>
    public class Netlist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Netlist"/> class.
        /// </summary>
        /// <param name="circuit">The SpiceSharp circuit for the netlist</param>
        /// <param name="title">The title of the netlist</param>
        public Netlist(Circuit circuit, string title)
        {
            Circuit = circuit;
            Title = title;
        }

        /// <summary>
        /// Gets the title of the netlist
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the circuit from the netlist
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// Gets the list of simulation from the netlist
        /// </summary>
        public List<Simulation> Simulations { get; } = new List<Simulation>();

        /// <summary>
        /// Gets the list of comments from the netlist
        /// </summary>
        public List<string> Comments { get; } = new List<string>();

        /// <summary>
        /// Gets the warnings created during creating SpiceSharp objects
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Gets the list of exports from the netlist
        /// </summary>
        public List<SpiceSharp.Parser.Readers.Export> Exports { get; } = new List<SpiceSharp.Parser.Readers.Export>();

        /// <summary>
        /// Gets the list of generated plots
        /// </summary>
        public List<Plot> Plots { get; } = new List<Plot>();
    }
}
