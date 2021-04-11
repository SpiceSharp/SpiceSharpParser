namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A parameter that has a single string value.
    /// </summary>
    public abstract class SingleParameter : Parameter
    {
        private readonly string _rawString;

        protected SingleParameter(string rawString, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            _rawString = rawString;
        }

        /// <summary>
        /// Gets the string representation of the parameter.
        /// </summary>
        public override string Image => _rawString;
    }
}