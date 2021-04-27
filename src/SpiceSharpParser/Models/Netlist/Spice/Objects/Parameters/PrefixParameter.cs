namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A prefix parameter.
    /// </summary>
    public class PrefixParameter : SingleParameter
    {
        public PrefixParameter(string prefix, SpiceLineInfo lineInfo)
            : base(prefix, lineInfo)
        {
        }

        public PrefixParameter(string prefix)
            : base(prefix, null)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new PrefixParameter(Value, LineInfo);
        }
    }
}