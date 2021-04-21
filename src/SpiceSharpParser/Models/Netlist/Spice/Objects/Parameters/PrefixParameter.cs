namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A prefix parameter.
    /// </summary>
    public class PrefixParameter : SingleParameter
    {
        public PrefixParameter(string word, SpiceLineInfo lineInfo)
            : base(word, lineInfo)
        {
        }

        public PrefixParameter(string word)
            : base(word, null)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new PrefixParameter(Image, LineInfo);
        }
    }
}