namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A spice model.
    /// </summary>
    public class Model : Statement
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets paramters of the model.
        /// </summary>
        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new Model()
            {
                Name = this.Name,
                LineNumber = this.LineNumber,
                Parameters = (ParameterCollection)this.Parameters.Clone(),
            };
        }
    }
}
