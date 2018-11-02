using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    using SpiceSharpParser.Common.Evaluation;

    /// <summary>
    /// Reads .PARAM <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class ParamControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            Read(statement, context.ExpressionParser, context.ReadingExpressionContext, context.CaseSensitivity);
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        public void Read(Control statement, IExpressionParser expressionParser,  ExpressionContext expressionContext, SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            foreach (var param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        string parameterName = assignmentParameter.Name;
                        string parameterExpression = assignmentParameter.Value;

                        expressionContext.SetParameter(
                            parameterName,
                            parameterExpression,
                            expressionParser.Parse(
                                parameterExpression,
                                new ExpressionParserContext(caseSettings.IsFunctionNameCaseSensitive)
                                    {
                                        Functions = expressionContext.Functions
                                    }).FoundParameters);
                    }
                    else
                    {
                        FunctionFactory factory = new FunctionFactory();

                        expressionContext.Functions.Add(
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
    }
}
