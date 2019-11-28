using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

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
            Read(statement, context.Evaluator.GetEvaluationContext(), true);
        }

        public void Read(Control statement, EvaluationContext context)
        {
            Read(statement, context, true);
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">Expression context.</param>
        /// <param name="validate">Validate.</param>
        public void Read(Control statement, EvaluationContext context, bool validate)
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
                            SetParameter(parameterName, parameterExpression, context);
                        }
                        catch (Exception e)
                        {
                            if (validate)
                            {
                                throw new ReadingException(
                                    $"Problem with setting param `{assignmentParameter.Name}` with expression =`{assignmentParameter.Value}` at line = {assignmentParameter.LineNumber}",
                                    e);
                            }
                        }
                    }
                    else
                    {
                        FunctionFactory factory = new FunctionFactory();

                        context.AddFunction(
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

        protected abstract void SetParameter(string parameterName, string parameterExpression, EvaluationContext context);
    }
}