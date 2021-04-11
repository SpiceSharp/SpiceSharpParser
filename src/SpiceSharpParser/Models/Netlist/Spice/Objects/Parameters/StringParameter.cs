namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A string parameter.
    /// </summary>
    public class StringParameter : SingleParameter
    {
        public StringParameter(string rawString, SpiceLineInfo lineInfo)
            : base(rawString, lineInfo)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new StringParameter(Image, LineInfo);
        }
    }
}