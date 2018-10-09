using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .FUNC <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class FuncControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            for (var i = 0; i < statement.Parameters.Count; i++)
            {
                var param = statement.Parameters[i];

                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter assigmentParameter)
                {
                    if (!assigmentParameter.HasFunctionSyntax)
                    {
                        throw new System.Exception("User function needs to be a function");
                    }

                    context.Evaluators.AddFunction(assigmentParameter.Name, assigmentParameter.Arguments, assigmentParameter.Value);
                }
                else
                {
                    if (param is Models.Netlist.Spice.Objects.Parameters.BracketParameter bracketParameter)
                    {
                        context.Evaluators.AddFunction(
                            bracketParameter.Name,
                            bracketParameter.Parameters.ToList().Select(p => p.Image).ToList(), // TODO: improve it please
                            statement.Parameters[i + 1].Image);

                        i++;
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
