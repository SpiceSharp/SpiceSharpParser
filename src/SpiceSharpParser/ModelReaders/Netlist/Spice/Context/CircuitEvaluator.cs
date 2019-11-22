using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class CircuitEvaluator : ICircuitEvaluator
    {
        public CircuitEvaluator(
            SimulationExpressionContexts simulationContexts,
            EvaluationContext parsingContext,
            IRandomizer randomizer)
        {
            SimulationContexts = simulationContexts;
            ParsingContext = parsingContext;
            Randomizer = randomizer;
        }

        public ICircuitEvaluator GetEvaluator(EvaluationContext parsingContext)
        {
            return new CircuitEvaluator(new SimulationExpressionContexts(parsingContext), parsingContext, Randomizer);
        }

        public IRandomizer Randomizer { get; }
        protected SimulationExpressionContexts SimulationContexts { get; }

        protected EvaluationContext ParsingContext { get; }

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>
        /// A value of expression..
        /// </returns>
        public double EvaluateDouble(string expression, Simulation sim)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            try
            {
                if (sim == null)
                {
                    return ParsingContext.Evaluate(expression);
                }

                return SimulationContexts.GetContext(sim).Evaluate(expression);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of expression: " + expression, ex);
            }
        }

        public double EvaluateDouble(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            try
            {
                return ParsingContext.Evaluate(expression);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of expression: " + expression, ex);
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
                return ParsingContext.Evaluate(parameter.Image);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of parameter: " + parameter.Image, ex);
            }
        }

        public void SetParameter(string parameterName, double value)
        {
            ParsingContext.SetParameter(parameterName, value);
        }

        public void SetParameter(Simulation simulation, string parameterName, double value)
        {
            SimulationContexts.GetContext(simulation).SetParameter(parameterName, value);
        }

        public void SetParameter(string parameterName, Parameter parameter)
        {
            ParsingContext.SetParameter(parameterName, parameter.Image);
        }

        public void SetParameter(string parameterName, string parameterExpression)
        {
            throw new NotImplementedException();
        }

        public void AddFunction(string functionName, List<string> arguments, string body)
        {
            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }
            IFunctionFactory factory = new FunctionFactory();
            ParsingContext.AddFunction(functionName, factory.Create(functionName, arguments, body));
        }

        public void AddFunction(string functionName, IFunction<double, double> function)
        {
            ParsingContext.AddFunction(functionName, function);
        }

        public void SetNamedExpression(string expressionName, string expression)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            ParsingContext.SetNamedExpression(expressionName, expression);
        }

        public EvaluationContext GetContext(Simulation simulation)
        {
            if (simulation != null)
            {
                return SimulationContexts.GetContext(simulation);
            }

            return ParsingContext;
        }

        public bool HaveParameter(Simulation simulation, string parameterName)
        {
            return GetContext(simulation).Parameters.ContainsKey(parameterName);
        }

        public EvaluationContext CreateChildContext(string name, bool b)
        {
            return ParsingContext.CreateChildContext(name, b);
        }

        public int? GetSeed(Simulation sim)
        {
            return SimulationContexts.GetContext(sim).Randomizer.Seed;
        }

        public Expression GetExpression(string expressionName)
        {
            return ParsingContext.GetExpression(expressionName);
        }

        public IEnumerable<string> GetExpressionNames()
        {
            return ParsingContext.GetExpressionNames();
        }

        private int? _seed;

        public int? Seed
        {
            get { return _seed; }

            set
            {
                _seed = value;

                ParsingContext.Seed = value;
            }
        }
    }
}