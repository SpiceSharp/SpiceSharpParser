using System;
using SpiceSharp;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// An parameter that triggers re-evaluation when changed.
    /// </summary>
    public class EvaluationParameter : Parameter<double>
    {
        private double _rawValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationParameter"/> class.
        /// </summary>
        /// <param name="context">An expression context.</param>
        /// <param name="parameterName">A parameter name.</param>
        public EvaluationParameter(ExpressionContext context, string parameterName)
        {
            ExpressionContext = context ?? throw new ArgumentNullException(nameof(context));
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        }

        /// <summary>
        /// Gets or sets the value of parameter.
        /// </summary>
        public override double Value
        {
            get => _rawValue;

            set
            {
                _rawValue = value;
                ExpressionContext.SetParameter(ParameterName, value);
            }
        }

        /// <summary>
        /// Gets the evaluator.
        /// </summary>
        protected ExpressionContext ExpressionContext { get; }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        protected string ParameterName { get; }

        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// A clone of parameter.
        /// </returns>
        public override Parameter<double> Clone()
        {
            return new EvaluationParameter(ExpressionContext, ParameterName);
        }
    }
}
