using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reades .NODESET <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class NodeSetControl : BaseControl
    {
        /// <summary>
        /// Gets name of Spice element
        /// </summary>
        public override string SpiceCommandName => "nodeset";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter ap)
                {
                    string type = ap.Name.ToLower();
                    string value = ap.Value;

                    if (type == "v" && ap.Arguments.Count == 1)
                    {
                        context.SetNodeSetVoltage(ap.Arguments[0], value);
                    }
                    else
                    {
                        throw new GeneralReaderException(".NODESET supports only V(X)=Y");
                    }
                }
            }
        }
    }
}
