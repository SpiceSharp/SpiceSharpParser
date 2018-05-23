using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls.Exporters;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls.Plots;

namespace SpiceSharpParser.ModelReader.Spice
{
    /// <summary>
    /// A result of reading Spice models
    /// </summary>
    public class SpiceReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceReaderResult"/> class.
        /// </summary>
        /// <param name="circuit">The SpiceSharp circuit for the netlist</param>
        /// <param name="title">The title of the netlist</param>
        public SpiceReaderResult(Circuit circuit, string title)
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
        public List<Export> Exports { get; } = new List<Export>();

        /// <summary>
        /// Gets the list of generated plots
        /// </summary>
        public List<Plot> Plots { get; } = new List<Plot>();
    }
}
