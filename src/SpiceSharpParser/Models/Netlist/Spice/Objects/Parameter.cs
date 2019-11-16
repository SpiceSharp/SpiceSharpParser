namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A base class for all parameters.
    /// </summary>
    public abstract class Parameter : SpiceObject
    {
        /// <summary>
        /// Gets the text representation of the parameter.
        /// </summary>
        public abstract string Image
        {
            get;
        }

        public int LineNumber { get; set; }

        public override string ToString()
        {
            return Image;
        }
    }
}
