using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls
{
    /// <summary>
    /// Processes .PARAM <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ParamControl : BaseControl
    {
        public override string TypeName => "param";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
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
                    if (!assigmentParameter.HasFunctionSyntax)
                    {
                        string name = assigmentParameter.Name;
                        string expression = assigmentParameter.Value;

                        //TODO: Please refactor this, there should be a better API for that
                        context.Evaluator.SetParameter(name, expression);
                        var dependedVariables = context.Evaluator.GetVariables(expression);
                        context.Evaluator.AddDynamicExpression(
                            new DoubleExpression(
                                expression,
                                (val) => context.Evaluator.SetParameter(name, expression)),
                            dependedVariables);
                    }
                    else
                    {
                        DefineUserFunction(context, assigmentParameter);
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
