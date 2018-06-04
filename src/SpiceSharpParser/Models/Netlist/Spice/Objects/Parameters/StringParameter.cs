namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A string parameter
    /// </summary>
    public class StringParameter : SingleParameter
    {
        public StringParameter(string rawString)
            : base(rawString)
        {
        }

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new StringParameter(this.Image);
        }
    }
}
