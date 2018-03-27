namespace SpiceNetlist.SpiceObjects
{
    /// <summary>
    /// A spice control
    /// </summary>
    public class Control : Statement
    {
        /// <summary>
        /// Gets or sets the name of control
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the paramaters of control
        /// </summary>
        public ParameterCollection Parameters { get; set; }
    }
}
