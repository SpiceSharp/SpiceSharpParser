namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A value parameter
    /// </summary>
    public class ValueParameter : SingleParameter
    {
        public ValueParameter(string value)
            : base(value)
        {
        }

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new ValueParameter(this.Image);
        }
    }
}
