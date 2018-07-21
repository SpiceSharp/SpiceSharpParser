using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Common
{
    /// <summary>
    /// An interface for all evaluators
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Gets the evaluator name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the children evaluators.
        /// </summary>
        List<IEvaluator> Children { get; }

        /// <summary>
        /// Gets the custom functions.
        /// </summary>
        Dictionary<string, CustomFunction> CustomFunctions { get; }

        /// <summary>
        /// Evaluates a specific expression to double.
        /// </summary>
        /// <param name="expressionString">An expression to evaluate.</param>
        /// <param name="context">Context of expression.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        double EvaluateDouble(string expressionString, object context = null);

        /// <summary>
        /// Gets the value of expression.
        /// </summary>
        /// <param name="expressionName">An expression name.</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double GetExpressionValue(string expressionName, object context);

        /// <summary>
        /// Gets the names of parameters.
        /// </summary>
        /// <returns>
        /// The names of paramaters.
        /// </returns>
        IEnumerable<string> GetParameterNames();

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
        double GetParameterValue(string parameterName, object context);

        /// <summary>
        /// Returns a value indicating whether there is a expression in evaluator with given name.
        /// </summary>
        /// <param name="expressionName">An expression name.</param>
        /// <returns>
        /// True if there is expression with given name.
        /// </returns>
        bool HasExpression(string expressionName);

        /// <summary>
        /// Returns a value indicating whether there is a parameter in evaluator with given name.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// True if there is a parameter with given name.
        /// </returns>
        bool HasParameter(string parameterName);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="expressionString">Parameter expression.</param>
        void SetParameter(string name, string expressionString, object context = null);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter expression.</param>
        void SetParameter(string name, double value, object context = null);

        /// <summary>
        /// Sets parameters.
        /// </summary>
        /// <param name="parameters">Dictionary of parameters to set.</param>
        void SetParameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A child evaluator.
        /// </returns>
        IEvaluator CreateChildEvaluator(string name);

        /// <summary>
        /// Creates a cloned evaluator.
        /// </summary>
        /// <returns>
        /// A cloned evaluator.
        /// </returns>
        IEvaluator CreateClonedEvaluator(string name);

        /// <summary>
        /// Invalidate parameters.
        /// </summary>
        void InvalidateParameters();

        /// <summary>
        /// Sets a custom function.
        /// </summary>
        /// <param name="functionName">Name of custom function</param>
        /// <param name="arguments">Arguments names of custom function</param>
        /// <param name="functionBody">Body of custom function</param>
        void AddCustomFunction(string functionName, List<string> arguments, string functionBody);

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
        /// Gets the parameters from expression.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>
        /// Collection of parameters.
        /// </returns>
        ICollection<string> GetParametersFromExpression(string expression);

        /// <summary>
        /// Refreshes all dependent expressions and parameters.
        /// </summary>
        /// <param name="parameterName">Paramter</param>
        void RefreshForParameter(string parameterName, object context);

        /// <summary>
        /// Finds a child evaluator with given name.
        /// </summary>
        /// <param name="name">Name of evaluator to find</param>
        /// <returns>
        /// A reference to evaluator.
        /// </returns>
        IEvaluator FindChildEvaluator(string name);

        /// <summary>
        /// Adds evaluator action.
        /// </summary>
        /// <param name="actionName">Action name.</param>
        /// <param name="expressionString">Expression.</param>
        /// <param name="expressionAction">Expression action.</param>
        void AddAction(string actionName, string expressionString, Action<double> expressionAction);
    }
}
