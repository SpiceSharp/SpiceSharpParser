using System;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Base class for parameter reading <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public abstract class ParamBaseControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            Read(statement, context.Evaluator.GetEvaluationContext(),  context.Result.Validation, true);
        }

        public void Read(Control statement, EvaluationContext context, SpiceNetlistValidationResult validation)
        {
            Read(statement, context, validation, true);
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">Expression context.</param>
        /// <param name="validation">Validation.</param>
        /// <param name="validate">Validate.</param>
        public void Read(Control statement, EvaluationContext context, SpiceNetlistValidationResult validation, bool validate)
        {
            if (statement.Parameters == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            foreach (Parameter param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        string parameterName = assignmentParameter.Name;
                        string parameterExpression = assignmentParameter.Value;

                        try
                        {
                            SetParameter(parameterName, parameterExpression, context);
                        }
                        catch (Exception e)
                        {
                            if (validate)
                            {
                                validation.Add(
                                    new ValidationEntry(
                                        ValidationEntrySource.Reader,
                                        ValidationEntryLevel.Warning,
                                        $"Problem with setting param `{assignmentParameter.Name}` with expression =`{assignmentParameter.Value}`",
                                        statement.LineInfo,
                                        exception: e));
                            }
                        }
                    }
                    else
                    {
                        FunctionFactory factory = new FunctionFactory();

                        context.AddFunction(
                            assignmentParameter.Name,
                            assignmentParameter.Value,
                            assignmentParameter.Arguments,
                            factory.Create(
                                assignmentParameter.Name,
                                assignmentParameter.Arguments,
                                assignmentParameter.Value));
                    }
                }
                else
                {
                    validation.Add(
                        new ValidationEntry(
                            ValidationEntrySource.Reader,
                            ValidationEntryLevel.Warning,
                            ".PARAM supports only assignments",
                            statement.LineInfo));
                }
            }
        }

        protected abstract void SetParameter(string parameterName, string parameterExpression, EvaluationContext context);
    }
}