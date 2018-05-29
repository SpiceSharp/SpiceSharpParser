using System.Linq;
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

            for (var i = 0; i < statement.Parameters.Count; i++)
            {
                var param = statement.Parameters[i];

                if (param is Model.Netlist.Spice.Objects.Parameters.AssignmentParameter assigmentParameter)
                {
                    if (!assigmentParameter.HasFunctionSyntax)
                    {
                        throw new System.Exception("User function needs to be a function");
                    }

                    DefineUserFunction(context, assigmentParameter.Name, assigmentParameter.Arguments, assigmentParameter.Value);
                    break;
                }
                else
                {
                    if (param is Model.Netlist.Spice.Objects.Parameters.BracketParameter bracketParameter)
                    {
                        DefineUserFunction(
                            context,
                            bracketParameter.Name,
                            bracketParameter.Parameters.ToList().Select(p => p.Image).ToList(), // TODO: improve it please
                            statement.Parameters[i + 1].Image);
                        break;
                    }
                    else
                    {
                        throw new WrongParameterTypeException("Unsupported syntax for .FUNC");
                    }
                }
            }
        }
    }
}
