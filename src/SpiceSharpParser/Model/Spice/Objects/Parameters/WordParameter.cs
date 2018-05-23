namespace SpiceSharpParser.Model.Spice.Objects.Parameters
{
    /// <summary>
    /// A word parameter
    /// </summary>
    public class WordParameter : SingleParameter
    {
        public WordParameter(string word)
            : base(word)
        {
        }
        
        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new WordParameter(this.Image);
        }
    }
}
