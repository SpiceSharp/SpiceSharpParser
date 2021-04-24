namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A SPICE component.
    /// </summary>
    public class Component : Statement
    {
        public Component(string name, ParameterCollection pinsAndParameters, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            PinsAndParameters = pinsAndParameters;
        }

        /// <summary>
        /// Gets the name of component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets name parameter.
        /// </summary>
        public Parameter NameParameter { get; set; }

        /// <summary>
        /// Gets pins and components parameters.
        /// </summary>
        public ParameterCollection PinsAndParameters { get; set; }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var clone = new Component(Name, (ParameterCollection)PinsAndParameters.Clone(), LineInfo);
            clone.NameParameter = (Parameter)NameParameter.Clone();
            return clone;
        }
    }
}