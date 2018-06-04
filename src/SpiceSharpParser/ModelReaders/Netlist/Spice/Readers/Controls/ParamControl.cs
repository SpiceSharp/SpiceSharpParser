using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reades .PARAM <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ParamControl : BaseControl
    {
        public override string SpiceName => "param";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            foreach (var param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter assigmentParameter)
                {
                    if (!assigmentParameter.HasFunctionSyntax)
                    {
                        string name = assigmentParameter.Name;
                        string expression = assigmentParameter.Value;

                        //TODO: Please refactor this, there should be a better API for that
                        context.Evaluator.SetParameter(name, expression);
                        var depParameters = context.Evaluator.GetParametersFromExpression(expression);
                        context.Evaluator.AddDynamicExpression(
                            new DoubleExpression(
                                expression,
                                (val) => context.Evaluator.SetParameter(name, expression)),
                            depParameters);
                    }
                    else
                    {
                        context.Evaluator.DefineCustomFunction(assigmentParameter.Name, assigmentParameter.Arguments, assigmentParameter.Value);
                    }
                }
                else
                {
                    throw new WrongParameterTypeException(".PARAM supports only assigments");
                }
            }
        }
    }
}
