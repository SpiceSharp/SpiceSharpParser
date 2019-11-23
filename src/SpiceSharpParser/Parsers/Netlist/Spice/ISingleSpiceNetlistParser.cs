using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// Interface for all SPICE netlist parsers.
    /// </summary>
    public interface ISingleSpiceNetlistParser
    {
        /// <summary>
        /// Gets or sets the parser settings.
        /// </summary>
        SingleSpiceNetlistParserSettings Settings { get; set; }

        /// <summary>
        /// Parses a SPICE netlist and returns a SPICE netlist model.
        /// </summary>
        /// <param name="tokens">SPICE netlist tokens.</param>
        /// <returns>
        /// A SPICE netlist model.
        /// </returns>
        SpiceNetlist Parse(SpiceToken[] tokens);
    }
}