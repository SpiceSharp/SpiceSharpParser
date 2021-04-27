namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A parameter that has a single value.
    /// </summary>
    public abstract class SingleParameter : Parameter
    {
        protected SingleParameter(string value, SpiceLineInfo lineInfo)
            : base(value, lineInfo)
        {
        }
    }
}