using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Text;

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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Title);

            for (var i = 0; i < Statements.LineInfo.LineNumber - 2; i++)
            {
                builder.AppendLine();
            }

            builder.AppendLine(Statements.ToString());
            builder.Append(".END");

            return builder.ToString();
        }
    }
}