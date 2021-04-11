namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// Base class for all SPICE statements.
    /// </summary>
    public abstract class Statement : SpiceObject
    {
        protected Statement(SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
        }
    }
}