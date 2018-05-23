namespace SpiceSharpParser.Model.Spice.Objects.Parameters
{
    /// <summary>
    /// An identifier parameter
    /// </summary>
    public class IdentifierParameter : SingleParameter
    {
        public IdentifierParameter(string identifier)
            : base(identifier)
        {
        }

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new IdentifierParameter(this.Image);
        }
    }
}
