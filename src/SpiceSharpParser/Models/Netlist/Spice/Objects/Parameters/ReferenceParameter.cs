namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A reference parameter.
    /// </summary>
    public class ReferenceParameter : SingleParameter
    {
        public ReferenceParameter(string reference, SpiceLineInfo lineInfo)
            : base(reference, lineInfo)
        {
            Name = reference.Substring(1, reference.IndexOf('[') - 1);
            Argument = reference.Substring(reference.IndexOf('[') + 1).TrimEnd(']');
        }

        /// <summary>
        /// Gets the name of reference parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the argument of reference parameter.
        /// </summary>
        public string Argument { get; }

        /// <summary>
        /// Gets the string representation of the point.
        /// </summary>
        public override string ToString()
        {
            return $"@{Name}[{Argument}]";
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new ReferenceParameter(Value, LineInfo);
        }
    }
}