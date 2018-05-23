using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.Model.Spice
{
    /// <summary>
    /// Spice netlist
    /// </summary>
    public class Netlist : SpiceObject
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
            return new Netlist() { Title = this.Title, Statements = (Statements)Statements.Clone() };
        }
    }
}
