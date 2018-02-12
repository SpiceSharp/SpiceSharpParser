using System.Collections.Generic;
using SpiceNetlist.SpiceObjects.Parameters;

namespace SpiceNetlist.SpiceObjects
{
    public class SubCircuit : Statement
    {
        public string Name { get; set; }

        public List<string> Pins { get; set; } = new List<string>();

        public List<AssignmentParameter> DefaultParameters { get; set; } = new List<AssignmentParameter>();

        public Statements Statements { get; set; }
    }
}
