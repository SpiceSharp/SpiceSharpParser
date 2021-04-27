namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A base class for all parameters.
    /// </summary>
    public abstract class Parameter : SpiceObject
    {
        protected Parameter(string value, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Value = value;
        }

        protected Parameter(string value)
        {
            Value = value;
        }

        protected Parameter(SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
        }

        protected Parameter()
        {
        }

        public virtual string Value { get; set; }

        /// <summary>
        /// Gets the string representation of the parameter.
        /// </summary>
        /// <returns>String representation of the parameter.</returns>
        public override string ToString()
        {
            return Value;
        }
    }
}