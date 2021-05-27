using System.Text;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    public class Parallel : Statement
    {
        public Parallel()
            : base(null)
        {
        }

        public Parallel(string name, Statements statements, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            Statements = statements;
        }

        /// <summary>
        /// Gets or sets statements of subcircuit.
        /// </summary>
        public Statements Statements { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var clone = new Parallel(Name, (Statements)Statements.Clone(), LineInfo);
            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($".PARALLEL {Name}");

            foreach (Statement statement in Statements)
            {
                builder.AppendLine(statement.ToString());
            }

            builder.AppendLine($".ENDP");

            return builder.ToString();
        }
    }
}