using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist
{
    /// <summary>
    /// Spice netlist
    /// </summary>
    public class NetList : SpiceObject
    {
        /// <summary>
        /// Gets or sets title of the netlist
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets collection of statements
        /// </summary>
        public Statements Statements { get; set; }
    }
}
