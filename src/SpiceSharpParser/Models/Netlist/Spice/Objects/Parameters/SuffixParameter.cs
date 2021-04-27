namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A suffix parameter.
    /// </summary>
    public class SuffixParameter : SingleParameter
    {
        public SuffixParameter(string suffix, SpiceLineInfo lineInfo)
            : base(suffix, lineInfo)
        {
        }

        public SuffixParameter(string suffix)
            : base(suffix, null)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new SuffixParameter(Value, LineInfo);
        }
    }
}