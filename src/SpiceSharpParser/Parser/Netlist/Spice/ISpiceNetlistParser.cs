using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Spice netlist parser
    /// </summary>
    public interface ISpiceNetlistParser
    {
        /// <summary>
        /// Parses a spice netlist and returns a spice model.
        /// </summary>
        /// <param name="spiceNetlist">Spice netlist to parse.</param>
        /// <param name="settings">Setting for parser.</param>
        /// <returns>
        /// A spice netlist model.
        /// </returns>
        SpiceNetlist Parse(string spiceNetlist, SpiceNetlistParserSettings settings);
    }
}
