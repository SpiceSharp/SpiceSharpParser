namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A SPICE model.
    /// </summary>
    public class Model : Statement
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets parameters of the model.
        /// </summary>
        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new Model()
            {
                Name = Name,
                LineNumber = LineNumber,
                Parameters = (ParameterCollection)Parameters.Clone(),
            };
        }
    }
}