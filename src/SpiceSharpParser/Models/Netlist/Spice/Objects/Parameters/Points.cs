using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class Points : SpiceObject, IEnumerable<PointParameter>
    {
        public Points(List<PointParameter> values, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Values = values;
        }

        public Points()
        {
            Values = new List<PointParameter>();
        }

        public List<PointParameter> Values { get; }

        public override SpiceLineInfo LineInfo => Values.FirstOrDefault()?.LineInfo;

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