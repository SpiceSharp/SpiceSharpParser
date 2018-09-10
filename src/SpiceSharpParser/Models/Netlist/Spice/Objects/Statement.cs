namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// Base class for all SPICE statements.
    /// </summary>
    public abstract class Statement : SpiceObject
    {
        /// <summary>
        /// Gets or sets the line number of the statement.
        /// </summary>
        public int? LineNumber { get; set; }
    }
}
