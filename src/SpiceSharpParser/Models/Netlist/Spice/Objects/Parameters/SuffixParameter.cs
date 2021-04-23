namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A suffix parameter.
    /// </summary>
    public class SuffixParameter : SingleParameter
    {
        public SuffixParameter(string word, SpiceLineInfo lineInfo)
            : base(word, lineInfo)
        {
        }

        public SuffixParameter(string word)
            : base(word, null)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new SuffixParameter(Image, LineInfo);
        }
    }
}