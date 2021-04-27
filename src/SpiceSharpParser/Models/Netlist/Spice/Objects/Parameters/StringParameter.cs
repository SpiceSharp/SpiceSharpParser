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
        /// Gets the string representation of the point.
        /// </summary>
        public override string ToString()
        {
            return @$"""{Value}""";
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new StringParameter(Value, LineInfo);
        }
    }
}