namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// An interface for all evaluators.
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Gets or sets the evaluator name.
        /// </summary>
        string Name { get; set; }

        bool IsParameterNameCaseSensitive { get; }

        bool IsFunctionNameCaseSensitive { get; }

        double EvaluateValueExpression(string expression, ExpressionContext context);

        double EvaluateNamedExpression(string expressionName, ExpressionContext context);

        double EvaluateParameter(string id, ExpressionContext context);

        IExpressionParser ExpressionParser { get; }
    }
}
