using System.Linq;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A point parameter.
    /// </summary>
    public class PointParameter : Parameter
    {
        /// <summary>
        /// Gets or sets the elements of the point.
        /// </summary>
        public PointValues Values { get; set; }

        /// <summary>
        /// Gets the string representation of the point.
        /// </summary>
        public override string Image
        {
            get
            {
                return string.Format("({0})", string.Join(",", Values.Items.Select( v => v.Image).ToArray()));
            }
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new ValueParameter(this.Image);
        }
    }
}
