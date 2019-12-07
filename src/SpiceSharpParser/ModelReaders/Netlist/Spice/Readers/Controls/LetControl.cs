using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .LET <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class LetControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement.Parameters.Count != 2)
            {
                throw new WrongParametersCountException("Wrong parameter count for .LET", statement.LineInfo);
            }

            if (!(statement.Parameters[1] is SingleParameter))
            {
                throw new WrongParameterTypeException("First parameter for .LET should be an single", statement.LineInfo);
            }

            string expressionName = statement.Parameters[0].Image;

            if (!(statement.Parameters[1] is ExpressionParameter) && !(statement.Parameters[1] is StringParameter))
            {
                throw new WrongParameterTypeException("Second parameter for .LET should be an expression", statement.LineInfo);
            }

            string expression = statement.Parameters.Get(1).Image;
            context.Evaluator.SetNamedExpression(expressionName, expression);
        }
    }
}