using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.Models.Netlist.Spice
{
    /// <summary>
    /// A SPICE netlist.
    /// </summary>
    public class SpiceNetlist : SpiceObject
    {
        public SpiceNetlist(string title, Statements statements)
        {
            Title = title;
            Statements = statements;
        }

        /// <summary>
        /// Gets title of the netlist.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets or sets a collection of statements of the netlist.
        /// </summary>
        public Statements Statements { get; set; }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new SpiceNetlist(Title, (Statements)Statements.Clone());
        }
    }
}