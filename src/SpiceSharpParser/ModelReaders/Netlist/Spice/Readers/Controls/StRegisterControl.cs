using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .ST_R <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StRegisterControl : BaseControl
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

            if (statement.Parameters.Count < 3)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Too less parameters for .ST_R",
                    statement.LineInfo);

                return;
            }

            string firstParam = statement.Parameters[0].Value;

            switch (firstParam.ToLower())
            {
                case "oct":
                case "dec":
                case "list":
                case "lin":
                    RegisterParameter(statement.Parameters.Skip(1), context);
                    break;

                default:
                    RegisterParameter(statement.Parameters, context);
                    break;
            }
        }

        private void RegisterParameter(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            context.EvaluationContext.SetParameter(variableParameter.Value, 0);
        }
    }
}