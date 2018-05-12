namespace SpiceSharpParser.Model.SpiceObjects
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

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new Control() {
                Name = this.Name,
                Parameters = (ParameterCollection)this.Parameters.Clone(),
                Comment = this.Comment,
                LineNumber = this.LineNumber
            };
        }
    }
}
