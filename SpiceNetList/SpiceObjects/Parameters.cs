using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects
{
    public class Parameters : SpiceObject
    {
        public List<Parameter> Values { get; set; }

        public Parameters()
        {
            Values = new List<Parameter>();
        }
    }
}
