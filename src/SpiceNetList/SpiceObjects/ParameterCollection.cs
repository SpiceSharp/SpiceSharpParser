using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects
{
    public class ParameterCollection : SpiceObject
    {
        public ParameterCollection()
        {
            Values = new List<Parameter>();
        }

        public List<Parameter> Values { get; set; }
    }
}
