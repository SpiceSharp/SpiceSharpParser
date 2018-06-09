using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Prints;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice
{
    /// <summary>
    /// A result of reading a spice netlist model.
    /// </summary>
    public class SpiceNetlistReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistReaderResult"/> class.
        /// </summary>
        /// <param name="circuit">The SpiceSharp circuit for the netlist.</param>
        /// <param name="title">The title of the netlist.</param>
        public SpiceNetlistReaderResult(Circuit circuit, string title)
        {
            Circuit = circuit;
            Title = title;
        }

        /// <summary>
        /// Gets the title of the netlist.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the circuit from the netlist.
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// Gets the list of simulation from the netlist.
        /// </summary>
        public List<Simulation> Simulations { get; } = new List<Simulation>();

        /// <summary>
        /// Gets the list of comments from the netlist.
        /// </summary>
        public List<string> Comments { get; } = new List<string>();

        /// <summary>
        /// Gets the warnings created during creating SpiceSharp objects.
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Gets the list of exports from the netlist.
        /// </summary>
        public List<Export> Exports { get; } = new List<Export>();

        /// <summary>
        /// Gets the list of generated plots.
        /// </summary>
        public List<Plot> Plots { get; } = new List<Plot>();

        /// <summary>
        /// Gets the list of generated prints.
        /// </summary>
        public List<Print> Prints { get; } = new List<Print>();

        /// <summary>
        /// Gets or sets used evaluator for simulations.
        /// </summary>
        public IEvaluator Evaluator { get; set; }
    }
}
