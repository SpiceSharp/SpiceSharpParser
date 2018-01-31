using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects
{
    public class ParameterCollection : SpiceObject
    {
        public List<Parameter> Values { get; set; }

        public ParameterCollection()
        {
            Values = new List<Parameter>();
        }
    }
}
