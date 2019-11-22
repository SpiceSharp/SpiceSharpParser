using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    /// <summary>
    /// Specifies random tolerance for a parameter.
    /// </summary>
    public class ParameterRandomness
    {
        /// <summary>
        /// Gets or sets random distribution name.
        /// </summary>
        public string RandomDistributionName { get; set; }

        /// <summary>
        /// Gets or sets tolerance parameter.
        /// </summary>
        public Parameter Tolerance { get; set; }
    }
}