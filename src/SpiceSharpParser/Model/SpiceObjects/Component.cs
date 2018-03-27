namespace SpiceSharpParser.Model.SpiceObjects
{
    /// <summary>
    /// A spice component
    /// </summary>
    public class Component : Statement
    {
        /// <summary>
        /// Gets or sets the name of component
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets pins and components parameters
        /// </summary>
        public ParameterCollection PinsAndParameters { get; set; }
    }
}
