using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Exceptions;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReader.Spice.Processors.Controls
{
    /// <summary>
    /// Processes .LET <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class LetControl : BaseControl
    {
        public override string TypeName => "let";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            if (statement.Parameters.Count != 2)
            {
                throw new WrongParametersCountException("Wrong parameter count for .let");
            }

            if (!(statement.Parameters[1] is SingleParameter))
            {
                throw new WrongParameterTypeException("First parameter for .let should be an single");
            }

            string expressionName = statement.Parameters[0].Image;

            if (!(statement.Parameters[1] is ExpressionParameter) && !(statement.Parameters[1] is StringParameter))
            {
                throw new WrongParameterTypeException("Second parameter for .let should be an expression");
            }

            string expression = statement.Parameters.GetString(1);
            var variables = context.Evaluator.GetVariables(expression);

            context.Evaluator.AddNamedDynamicExpression(expressionName, new Evaluation.DoubleExpression(expression, (newVal) => { }), variables);
        }
    }
}
