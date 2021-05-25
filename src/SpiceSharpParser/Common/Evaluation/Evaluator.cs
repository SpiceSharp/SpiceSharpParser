using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.Common.Evaluation
{
    public class Evaluator : IEvaluator
    {
        private readonly IEvaluationContext _evaluationContext;

        public Evaluator(IEvaluationContext evaluationContext, IExpressionValueProvider expressionValueProvider)
        {
            _evaluationContext = evaluationContext;
            _evaluationContext.Evaluator = this;
            ExpressionValueProvider = expressionValueProvider;
        }

        public IExpressionValueProvider ExpressionValueProvider { get; }

        public double EvaluateDouble(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            try
            {
                return ExpressionValueProvider.GetExpressionValue(expression, _evaluationContext);
            }
            catch (SpiceSharpParserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SpiceSharpParserException($"Exception during evaluation of expression: {expression}", ex);
            }
        }

        public double EvaluateDouble(Parameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            try
            {
                return EvaluateDouble(parameter.Value);
            }
            catch (Exception ex)
            {
                throw new SpiceSharpParserException($"Exception during evaluation of parameter: {parameter.Value}", ex);
            }
        }

        public double EvaluateDouble(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression.CanProvideValueDirectly)
            {
                return expression.GetValue();
            }

            return EvaluateDouble(expression.ValueExpression);
        }
    }
}