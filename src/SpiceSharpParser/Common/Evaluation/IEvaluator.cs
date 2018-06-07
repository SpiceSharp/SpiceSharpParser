using System.Collections.Generic;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.Common
{
    /// <summary>
    /// An interface for all evaluators
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Gets the custom functions.
        /// </summary>
        Dictionary<string, CustomFunction> CustomFunctions { get; }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change.
        /// </summary>
        /// <param name="expression">An expression to add.</param>
        /// <param name="parameters">Parameters of expression.</param>
        void AddActionExpression(ActionExpression expression, IEnumerable<string> parameters);

        /// <summary>
        /// Adds let expression to registry.
        /// </summary>
        /// <param name="expressionName">An expression name.</param>
        /// <param name="expression">An expression value.</param>
        /// <param name="parameters">Parameters of expression.</param>
        void AddNamedActionExpression(string expressionName, ActionExpression expression, IEnumerable<string> parameters);

        /// <summary>
        /// Evaluates a specific string to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <param name="context">Context of expression.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        double EvaluateDouble(string expression, object context = null);

        /// <summary>
        /// Sets the parameter value and updates the values expressions.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        void SetParameter(string parameterName, double value);

        /// <summary>
        /// Sets the parameter value and updates the values expressions
        /// </summary>
        /// <param name="parameterName">A name of parameter</param>
        /// <param name="expression">A parameter expression </param>
        void SetParameter(string parameterName, string expression);

        /// <summary>
        /// Sets the parameters values and updates the values expressions.
        /// </summary>
        /// <param name="parameters">A dictionary of parameter values.</param>
        void SetParameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Returns a value indicating whether there is a parameter in evaluator with given name.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// True if there is parameter.
        /// </returns>
        bool HasParameter(string parameterName);

        /// <summary>
        /// Gets the value of parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        /// <returns>
        /// A value of parameter.
        /// </returns>
        double GetParameterValue(string parameterName, object context);

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="expressionName">A expression name.</param>
        /// <returns>
        /// An expression.
        /// </returns>
        string GetExpression(string expressionName);

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
        /// Gets the variables in expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>
        /// A list of variables from expression.
        /// </returns>
        IEnumerable<string> GetParametersFromExpression(string expression);

        /// <summary>
        /// Invalidate parameters.
        /// </summary>
        void InvalidateParameters();

        /// <summary>
        /// Returns names of all parameters.
        /// </summary>
        /// <returns>
        /// True if there is parameter.
        /// </returns>
        IEnumerable<string> GetParameters();

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A child evaluator.
        /// </returns>
        IEvaluator CreateChildEvaluator();

        /// <summary>
        /// Defines a custom function.
        /// </summary>
        /// <param name="name">Name of custom function</param>
        /// <param name="arguments">Arguments names of custom function</param>
        /// <param name="functionBody">Body of custom function</param>
        void DefineCustomFunction(string name, List<string> arguments, string functionBody);
    }
}
