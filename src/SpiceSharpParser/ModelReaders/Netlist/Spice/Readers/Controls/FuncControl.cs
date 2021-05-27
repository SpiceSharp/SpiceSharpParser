using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

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

                if (param is AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        context.Result.ValidationResult.AddError(
                            ValidationEntrySource.Reader,
                            $".FUNC needs to be a function",
                            statement.LineInfo);
                        continue;
                    }

                    context.EvaluationContext.AddFunction(assignmentParameter.Name, assignmentParameter.Arguments, assignmentParameter.Value);
                }
                else
                {
                    if (param is BracketParameter bracketParameter)
                    {
                        var arguments = new List<string>();

                        if (bracketParameter.Parameters[0] is VectorParameter vp)
                        {
                            arguments.AddRange(vp.Elements.Select(element => element.Value));
                        }
                        else
                        {
                            if (bracketParameter.Parameters.Count != 0)
                            {
                                arguments.Add(bracketParameter.Parameters[0].Value);
                            }
                        }

                        context.EvaluationContext.AddFunction(
                            bracketParameter.Name,
                            arguments,
                            statement.Parameters[i + 1].Value);

                        i++;
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Unsupported syntax for .FUNC", param.LineInfo);
                    }
                }
            }
        }
    }
}