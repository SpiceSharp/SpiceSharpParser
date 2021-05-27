using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .STEP_R <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StepRegisterControl : BaseControl
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

            if (statement.Parameters.Count < 4)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Too less parameters for .STEP_R",
                    statement.LineInfo);
            }

            string firstParam = statement.Parameters[0].Value;

            switch (firstParam.ToLower())
            {
                case "param":
                    RegisterParameter(statement.Parameters.Skip(1)[0], context);
                    break;

                case "lin":
                    ReadLin(statement.Parameters.Skip(1), context);
                    break;

                case "dec":
                    ReadDec(statement.Parameters.Skip(1), context);
                    break;

                case "oct":
                    ReadOct(statement.Parameters.Skip(1), context);
                    break;

                default:
                    ReadOtherCases(statement.Parameters, context);
                    break;
            }
        }

        private void ReadOtherCases(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }

        private void ReadOct(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }

        private void ReadDec(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }

        private void ReadLin(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }

        private void RegisterParameter(Parameter variableParameter, IReadingContext context)
        {
            context.EvaluationContext.SetParameter(variableParameter.Value, 0);
        }
    }
}