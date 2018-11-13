using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class Points : SpiceObject, IEnumerable<PointParameter>
    {
        public List<PointParameter> Values { get; set; } = new List<PointParameter>();

        public override SpiceObject Clone()
        {
            return new Points();
        }

        public IEnumerator<PointParameter> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}
