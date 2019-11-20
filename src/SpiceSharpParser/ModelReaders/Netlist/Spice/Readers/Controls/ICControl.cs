using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .IC <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class ICControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter ap)
                {
                    string type = ap.Name.ToLower();
                    string initialValue = ap.Value;

                    if (type == "v" && ap.Arguments.Count == 1)
                    {
                        context.SetICVoltage(ap.Arguments[0], initialValue);
                    }
                    else
                    {
                        throw new GeneralReaderException(".ic supports only V(X)=Y");
                    }
                }
            }
        }
    }
}
