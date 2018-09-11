namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A percent parameter.
    /// </summary>
    public class PercentParameter : SingleParameter
    {
        public PercentParameter(string value)
            : base(value)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new PercentParameter(this.Image);
        }
    }
}
