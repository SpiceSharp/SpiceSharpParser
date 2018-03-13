using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    /// <summary>
    /// Processes .IC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ICControl : BaseControl
    {
        /// <summary>
        /// Gets name of Spice element
        /// </summary>
        public override string TypeName => "ic";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContextBase context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is SpiceObjects.Parameters.AssignmentParameter ap)
                {
                    string type = ap.Name.ToLower();
                    string initialValue = ap.Value;

                    if (type == "v" && ap.Arguments.Count == 1)
                    {
                        context.SetICVoltage(ap.Arguments[0], initialValue);
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                }
            }
        }
    }
}
