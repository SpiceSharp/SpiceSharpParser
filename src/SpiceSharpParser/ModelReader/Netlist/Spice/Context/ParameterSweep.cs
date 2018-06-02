using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Context
{
    /// <summary>
    /// A parameter sweep.
    /// </summary>
    public class ParameterSweep
    {
        /// <summary>
        /// Gets or sets the parameter for parameter sweep.
        /// </summary>
        public Model.Netlist.Spice.Objects.Parameter Parameter { get; set; }

        /// <summary>
        /// Gets or sets the sweep for parameter sweep.
        /// </summary>
        public Sweep<double> Sweep { get; set; }
    }
}
