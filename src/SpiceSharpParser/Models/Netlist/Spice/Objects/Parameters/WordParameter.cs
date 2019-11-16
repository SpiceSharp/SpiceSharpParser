namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A word parameter.
    /// </summary>
    public class WordParameter : SingleParameter
    {
        public WordParameter(string word)
            : base(word)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new WordParameter(Image) { LineNumber = LineNumber };
        }
    }
}
