using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls
{
    /// <summary>
    /// Processes .FUNC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class FuncControl : BaseControl
    {
        public override string TypeName => "func";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            foreach (var param in statement.Parameters)
            {
                if (param is Model.Netlist.Spice.Objects.Parameters.AssignmentParameter assigmentParameter)
                {
                    DefineUserFunction(context, assigmentParameter);
                    break;
                }
                else
                {
                    throw new WrongParameterTypeException(".FUNC supports only single assigment");
                }
            }
        }
    }
}
