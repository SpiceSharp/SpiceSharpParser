using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class PointValues : SpiceObject, IEnumerable<SingleParameter>
    {
        public List<SingleParameter> Items { get; set; } = new List<SingleParameter>();

        public override SpiceObject Clone()
        {
            return new Points();
        }

        public IEnumerator<SingleParameter> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
