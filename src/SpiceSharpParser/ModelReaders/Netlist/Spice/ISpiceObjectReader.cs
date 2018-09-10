namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Common
{
    /// <summary>
    /// Describes a reader of SPICE object.
    /// </summary>
    public interface ISpiceObjectReader
    {
        /// <summary>
        /// Gets the name of read SPICE object.
        /// </summary>
        string SpiceCommandName { get; }
    }
}
