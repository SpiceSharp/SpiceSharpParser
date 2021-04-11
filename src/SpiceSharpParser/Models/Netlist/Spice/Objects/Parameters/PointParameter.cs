using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A point parameter.
    /// </summary>
    public class PointParameter : Parameter
    {
        public PointParameter(PointValues values, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Values = values;
        }

        /// <summary>
        /// Gets the elements of the point.
        /// </summary>
        public PointValues Values { get; }

        /// <summary>
        /// Gets the string representation of the point.
        /// </summary>
        public override string Image
        {
            get
            {
                return $"({string.Join(",", Values.Items.Select(v => v.Image).ToArray())})";
            }
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var result = new PointParameter((PointValues)Values.Clone(), LineInfo);
            return result;
        }
    }
}