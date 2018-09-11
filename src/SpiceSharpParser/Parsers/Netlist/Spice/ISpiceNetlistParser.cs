using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// SPICE netlist parser
    /// </summary>
    public interface ISpiceNetlistParser
    {
        /// <summary>
        /// Parses a SPICE netlist and returns a SPICE netlist model.
        /// </summary>
        /// <param name="spiceNetlist">SPICE netlist to parse.</param>
        /// <param name="settings">Setting for parser.</param>
        /// <returns>
        /// A SPICE netlist model.
        /// </returns>
        SpiceNetlist Parse(string spiceNetlist, SpiceNetlistParserSettings settings);
    }
}
