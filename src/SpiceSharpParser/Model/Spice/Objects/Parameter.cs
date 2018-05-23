namespace SpiceSharpParser.Model.Spice.Objects
{
    /// <summary>
    /// A base class for all paramters
    /// </summary>
    public abstract class Parameter : SpiceObject
    {
        /// <summary>
        /// Gets the text representation of the paramter
        /// </summary>
        public abstract string Image
        {
            get;
        }
    }
}
