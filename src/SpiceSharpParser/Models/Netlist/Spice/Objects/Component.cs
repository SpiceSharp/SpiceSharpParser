namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A SPICE component
    /// </summary>
    public class Component : Statement
    {
        /// <summary>
        /// Gets or sets the name of component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets pins and components parameters.
        /// </summary>
        public ParameterCollection PinsAndParameters { get; set; }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new Component()
            {
                Name = this.Name,
                PinsAndParameters = (ParameterCollection)this.PinsAndParameters.Clone(),
                LineNumber = this.LineNumber,
            };
        }
    }
}
