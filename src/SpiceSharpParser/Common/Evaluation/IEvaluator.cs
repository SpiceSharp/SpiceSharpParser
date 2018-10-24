using System.Collections.Generic;

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

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        Dictionary<string, Expression> Parameters { get;  }

        /// <summary>
        /// Gets custom functions.
        /// </summary>
        Dictionary<string, Function> Functions { get;  }

        /// <summary>
        /// Gets the children evaluators.
        /// </summary>
        List<IEvaluator> Children { get; }

        /// <summary>
        /// Gets or sets the random seed.
        /// </summary>
        int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the context of the evaluator.
        /// </summary>
        object Context { get; set; }

        /// <summary>
        /// Evaluates a specific expression to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        double EvaluateDouble(string expression);

        /// <summary>
        /// Gets the names of expressions.
        /// </summary>
        /// <returns>
        /// The names of expressions.
        /// </returns>
        IEnumerable<string> GetExpressionNames();

        /// <summary>
        /// Gets the value of parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// A value of parameter.
        /// </returns>
        double GetParameterValue(string parameterName);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="expression">Parameter expression.</param>
        void SetParameter(string parameterName, string expression);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Parameter expression.</param>
        void SetParameter(string parameterName, double value);

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A child evaluator.
        /// </returns>
        IEvaluator CreateChildEvaluator(string name, object context);

        /// <summary>
        /// Sets a named expression.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <param name="expression">Expression.</param>
        void SetNamedExpression(string expressionName, string expression);

        /// <summary>
        /// Gets the expression.
        /// </summary>
        string GetExpression(string expressionName);

        /// <summary>
        /// Finds a child evaluator with given name.
        /// </summary>
        /// <param name="name">Name of evaluator to find</param>
        /// <returns>
        /// A reference to evaluator.
        /// </returns>
        IEvaluator FindChildEvaluator(string name);

        /// <summary>
        /// Clones the evaluator.
        /// </summary>
        /// <param name="deep">Specifies whether cloning is deep.</param>
        /// <returns>
        /// A clone of evaluator.
        /// </returns>
        IEvaluator Clone(bool deep);

        /// <summary>
        /// Gets found functions from expression.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <returns>
        /// A collection of names of functions found in expression.
        /// </returns>
        ICollection<string> GetFunctionsFromExpression(string expression);

        /// <summary>
        /// Gets a value indicating whether expression is constant expression.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <returns>
        /// A value indicating whether expression is constant expression.
        /// </returns>
        bool IsConstantExpression(string expression);
    }
}
