using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Netlist model reader
    /// </summary>
    public interface INetlistModelReader
    {
        /// <summary>
        /// Gets the netlist model
        /// </summary>
        /// <param name="netlist">Netlist to parse</param>
        /// <param name="settings">Setting for parser</param>
        /// <returns>
        /// A netlist model
        /// </returns>
        SpiceNetlist GetNetlistModel(string netlist, ParserSettings settings);
    }
}
