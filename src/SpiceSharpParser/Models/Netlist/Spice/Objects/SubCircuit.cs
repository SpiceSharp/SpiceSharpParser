using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

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
    }
}