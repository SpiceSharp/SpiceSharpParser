namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// An identifier parameter.
    /// </summary>
    public class IdentifierParameter : SingleParameter
    {
        public IdentifierParameter(string identifier)
            : base(identifier, null)
        {
        }

        public IdentifierParameter(string identifier, SpiceLineInfo lineInfo)
            : base(identifier, lineInfo)
        {
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new IdentifierParameter(Value, LineInfo);
        }
    }
}