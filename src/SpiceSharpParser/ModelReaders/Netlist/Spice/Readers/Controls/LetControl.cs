using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
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
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters.Count != 2)
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Wrong parameter count for .LET", statement.LineInfo);
                return;
            }

            if (!(statement.Parameters[1] is SingleParameter))
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "First parameter for .LET should be an single", statement.LineInfo);
                return;
            }

            string expressionName = statement.Parameters[0].Value;

            if (!(statement.Parameters[1] is ExpressionParameter) && !(statement.Parameters[1] is StringParameter))
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Second parameter for .LET should be an expression", statement.LineInfo);
                return;
            }

            string expression = statement.Parameters.Get(1).Value;
            context.EvaluationContext.SetNamedExpression(expressionName, expression);
        }
    }
}