using System;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
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
        public override void Read(Control statement, IReadingContext context)
        {
            Read(statement, context.ReadingExpressionContext, context.CaseSensitivity, context.ReadingEvaluator, context, true);
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="expressionContext">Context.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <param name="evaluator">Evaluator.</param>
        /// <param name="readingContext">Reading context.</param>
        /// <param name="validate"></param>
        public void Read(Control statement, ExpressionContext expressionContext, SpiceNetlistCaseSensitivitySettings caseSettings, IEvaluator evaluator, IReadingContext readingContext, bool validate)
        {
            if (statement.Parameters == null)
            {
                throw new ArgumentNullException(nameof(statement.Parameters));
            }

            foreach (var param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        string parameterName = assignmentParameter.Name;
                        string parameterExpression = assignmentParameter.Value;

                        try
                        {
                            SetParameter(parameterName, parameterExpression, expressionContext, evaluator, caseSettings, readingContext);
                        }
                        catch (Exception e)
                        {
                            if (validate)
                            {
                                throw new Exception(
                                    $"Problem with setting param `{assignmentParameter.Name}` with expression =`{assignmentParameter.Value}` at line = {assignmentParameter.LineNumber}",
                                    e);
                            }
                        }
                    }
                    else
                    {
                        FunctionFactory factory = new FunctionFactory();

                        expressionContext.AddFunction(
                            assignmentParameter.Name,
                            factory.Create(
                                assignmentParameter.Name,
                                assignmentParameter.Arguments,
                                assignmentParameter.Value));
                    }
                }
                else
                {
                    throw new WrongParameterTypeException(".PARAM supports only assignments");
                }
            }
        }

        protected abstract void SetParameter(
            string parameterName,
            string parameterExpression,
            ExpressionContext expressionContext,
            IEvaluator evaluator,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            IReadingContext readingContext);
    }
}
