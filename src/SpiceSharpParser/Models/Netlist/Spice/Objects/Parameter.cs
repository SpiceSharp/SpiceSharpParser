namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A base class for all parameters.
    /// </summary>
    public abstract class Parameter : SpiceObject
    {
        protected Parameter(SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
        }

        protected Parameter()
        {
        }

        /// <summary>
        /// Gets the text representation of the parameter.
        /// </summary>
        public abstract string Image
        {
            get;
        }

        /// <summary>
        /// Gets the string representation of the parameter.
        /// </summary>
        /// <returns>String representation of the parameter.</returns>
        public override string ToString()
        {
            return Image;
        }
    }
}