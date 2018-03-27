using System.Collections.Generic;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Model.SpiceObjects
{
    public class SubCircuit : Statement
    {
        /// <summary>
        /// Gets or sets the name of subcircuit
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets list of outside pins of subcircuit
        /// </summary>
        public List<string> Pins { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the default values of parameters
        /// </summary>
        public List<AssignmentParameter> DefaultParameters { get; set; } = new List<AssignmentParameter>();

        /// <summary>
        /// Gets or sets statements of subcircuit
        /// </summary>
        public Statements Statements { get; set; }
    }
}
