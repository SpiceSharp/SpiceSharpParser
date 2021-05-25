using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps
{
    /// <summary>
    /// A parameter sweep.
    /// </summary>
    public class ParameterSweep
    {
        /// <summary>
        /// Gets or sets the parameter for parameter sweep.
        /// </summary>
        public SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter Parameter { get; set; }

        /// <summary>
        /// Gets or sets the sweep for parameter sweep.
        /// </summary>
        public IEnumerable<double> Sweep { get; set; }
    }
}