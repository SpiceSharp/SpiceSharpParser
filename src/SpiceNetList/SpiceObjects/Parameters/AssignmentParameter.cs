using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects.Parameters
{
    public class AssignmentParameter : Parameter
    {
        public string Name { get; set; }

        public List<string> Arguments { get; set; } = new List<string>();

        public string Value { get; set; }
    }
}
