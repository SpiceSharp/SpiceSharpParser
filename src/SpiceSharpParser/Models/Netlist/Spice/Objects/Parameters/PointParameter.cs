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
        public SingleParameter X { get; set; }

        public SingleParameter Y { get; set; } 

        /// <summary>
        /// Gets the string represenation of the point.
        /// </summary>
        public override string Image
        {
            get
            {
                return string.Format("({0},{1})", X.Image, Y.Image);
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
