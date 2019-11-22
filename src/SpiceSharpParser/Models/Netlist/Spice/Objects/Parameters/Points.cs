using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class Points : SpiceObject, IEnumerable<PointParameter>
    {
        public List<PointParameter> Values { get; set; } = new List<PointParameter>();

        public override SpiceObject Clone()
        {
            var result = new Points();

            foreach (var value in Values)
            {
                result.Values.Add((PointParameter)value.Clone());
            }

            return result;
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