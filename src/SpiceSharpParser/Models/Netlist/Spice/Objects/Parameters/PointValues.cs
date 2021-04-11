using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class PointValues : SpiceObject, IEnumerable<SingleParameter>
    {
        public PointValues(List<SingleParameter> items, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Items = items;
        }

        public PointValues(SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Items = new List<SingleParameter>();
        }

        public List<SingleParameter> Items { get; }

        public override SpiceObject Clone()
        {
            var result = new PointValues(LineInfo);

            foreach (var item in Items)
            {
                result.Items.Add((SingleParameter)item.Clone());
            }

            return result;
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