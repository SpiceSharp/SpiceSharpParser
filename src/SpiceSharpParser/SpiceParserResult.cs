using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// A result of the SPICE netlist parser.
    /// </summary>
    public class SpiceParserResult
    {
        /// <summary>
        /// Gets or sets the result of reading <see cref="SpiceModel{TCircuit,TSimulation}"/> model.
        /// </summary>
        public ISpiceModel<Circuit, Simulation> SpiceModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model before preprocessing.
        /// </summary>
        public SpiceNetlist OriginalInputModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model after preprocessing.
        /// </summary>
        public SpiceNetlist PreprocessedInputModel { get; set; }
    }
}