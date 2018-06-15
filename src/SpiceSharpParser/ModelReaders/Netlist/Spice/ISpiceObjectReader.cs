namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Common
{
    /// <summary>
    /// Describes a reader of spice object.
    /// </summary>
    public interface ISpiceObjectReader
    {
        /// <summary>
        /// Gets the name of read spice object.
        /// </summary>
        string SpiceCommandName { get; }
    }
}
