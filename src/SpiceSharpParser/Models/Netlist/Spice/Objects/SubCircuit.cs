using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    public class SubCircuit : Statement
    {
        public SubCircuit()
            : base(null)
        {
        }

        public SubCircuit(string name, Statements statements, ParameterCollection pins, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            Statements = statements;
            Pins = pins;
        }

        /// <summary>
        /// Gets or sets the name of subcircuit.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets list of outside pins of subcircuit.
        /// </summary>
        public ParameterCollection Pins { get; set; }

        /// <summary>
        /// Gets or sets the default values of parameters.
        /// </summary>
        public List<AssignmentParameter> DefaultParameters { get; set; } = new List<AssignmentParameter>();

        /// <summary>
        /// Gets or sets statements of subcircuit.
        /// </summary>
        public Statements Statements { get; set; }

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        public override int EndLineNumber => Statements.LastOrDefault()?.EndLineNumber + 1 ?? base.EndLineNumber;

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var clone = new SubCircuit(Name, (Statements)Statements.Clone(), (ParameterCollection)Pins.Clone(), LineInfo);

            foreach (AssignmentParameter defaultParameter in DefaultParameters)
            {
                clone.DefaultParameters.Add((AssignmentParameter)defaultParameter.Clone());
            }

            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append($".SUBCKT {Name} {Pins} params:");

            foreach (var defaultParameter in DefaultParameters)
            {
                if (DefaultParameters.IndexOf(defaultParameter) != 0)
                {
                    builder.Append($", {defaultParameter}");
                }
                else
                {
                    builder.Append($" {defaultParameter}");
                }
            }

            builder.AppendLine();
            builder.AppendLine(Statements.ToString());
            builder.AppendLine($".ENDS {Name}");

            return builder.ToString();
        }
    }
}
