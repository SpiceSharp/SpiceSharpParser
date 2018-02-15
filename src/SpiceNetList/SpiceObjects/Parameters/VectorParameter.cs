using SpiceNetlist.SpiceObjects.Parameters;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects
{
    public class VectorParameter : Parameter
    {
        public List<SingleParameter> Elements { get; set; } = new List<SingleParameter>();
    }
}
