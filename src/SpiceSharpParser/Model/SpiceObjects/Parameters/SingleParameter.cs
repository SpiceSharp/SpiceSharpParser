namespace SpiceSharpParser.Model.SpiceObjects.Parameters
{
    /// <summary>
    /// A parameter that has a single string value
    /// </summary>
    public abstract class SingleParameter : Parameter
    {
        private string rawString;

        public SingleParameter(string rawString)
        {
            this.rawString = rawString;
        }

        /// <summary>
        /// Gets the string represenation of the parameter
        /// </summary>
        public override string Image => this.rawString;
    }
}
