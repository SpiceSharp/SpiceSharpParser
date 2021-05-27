using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .NODESET <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class NodeSetControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
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
                        var nodeName = ap.Arguments[0];
                        context.SetNodeSetVoltage(nodeName, value);
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, ".NODESET supports only V(X)=Y", statement.LineInfo);
                    }
                }
            }
        }
    }
}