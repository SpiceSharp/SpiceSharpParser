namespace SpiceNetlist.SpiceObjects
{
    /// <summary>
    /// A spice model
    /// </summary>
    public class Model : Statement
    {
        /// <summary>
        /// Gets or sets the name of the model
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets paramters of the model
        /// </summary>
        public ParameterCollection Parameters { get; set; }
    }
}
