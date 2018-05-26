using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.Model.Netlist.Spice
{
    /// <summary>
    /// Spice netlist
    /// </summary>
    public class SpiceNetlist : SpiceObject
    {
        /// <summary>
        /// Gets or sets title of the netlist
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets collection of statements
        /// </summary>
        public Statements Statements { get; set; }

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new SpiceNetlist() { Title = this.Title, Statements = (Statements)Statements.Clone() };
        }
    }
}
