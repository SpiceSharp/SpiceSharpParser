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
        Simulation Simulation { get; set; }

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
        void SetParameter(string name, string expressionString);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter expression.</param>
        void SetParameter(string name, double value);

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
        IEvaluator CreateChildEvaluator();

        /// <summary>
        /// Creates a cloned evaluator.
        /// </summary>
        /// <returns>
        /// A cloned evaluator.
        /// </returns>
        IEvaluator CreateClonedEvaluator();

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
        /// Sets expression action.
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="expression">Expression</param>
        /// <param name="actionWhenExpressionValueIsChanged">Action to run when expression is changed</param>
        void AddAction(string actionName, string expression, Action<Simulation, double> actionWhenExpressionValueIsChanged);

        /// <summary>
        /// Refreshes all dependent expressions and parameters.
        /// </summary>
        /// <param name="parameterName">Paramter</param>
        void RefreshForParameter(string parameterName);
    }
}
