using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ICircuitEvaluator
    {
        ICircuitEvaluator GetEvaluator(ExpressionContext parsingContext);

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double EvaluateDouble(string expression, Simulation sim);

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double EvaluateDouble(string expression);

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="parameter">Parameter to parse</param>
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

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        void SetParameter(string parameterName, string parameterExpression);

        void AddFunction(string functionName, List<string> arguments, string body);

        void AddFunction(string name, IFunction<double, double> function);

        void SetNamedExpression(string expressionName, string expression);

        ExpressionContext GetContext(Simulation simulation = null);

        bool HaveParameter(Simulation simulation, string parameterName);
        
        ExpressionContext CreateChildContext(string subcircuitFullName, bool b);
        
        int? GetSeed(Simulation sim);
        Expression GetExpression(string expressionName);
        IEnumerable<string> GetExpressionNames();
        int? Seed { get; set; }
    }
}
