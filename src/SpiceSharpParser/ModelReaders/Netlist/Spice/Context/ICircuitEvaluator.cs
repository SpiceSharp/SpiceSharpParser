using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ICircuitEvaluator
    {
        int? Seed { get; set; }

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <param name="simulation">Simulation.</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double EvaluateDouble(string expression, Simulation simulation);

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression.</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double EvaluateDouble(string expression);

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="parameter">Parameter.</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double EvaluateDouble(Parameter parameter);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        void SetParameter(string parameterName, double value);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="simulation">Simulation.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        void SetParameter(Simulation simulation, string parameterName, double value);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameter">Parameter value.</param>
        void SetParameter(string parameterName, Parameter parameter);

        void SetEntites(Circuit contextEntities);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="expression">Expression.</param>
        void SetParameter(string parameterName, string expression);

        void AddFunction(string functionName, List<string> arguments, string body);

        void AddFunction(string name, IFunction<double, double> function);

        void SetNamedExpression(string expressionName, string expression);

        EvaluationContext GetEvaluationContext(Simulation simulation = null);

        bool HaveParameter(Simulation simulation, string parameterName);

        EvaluationContext CreateChildContext(string name, bool addToChildren);

        int? GetSeed(Simulation sim);

        Expression GetExpression(string expressionName);

        IEnumerable<string> GetExpressionNames();
    }
}