namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// An parameter that triggers re-evaluation when changed
    /// </summary>
    public class EvaluationParameter : SpiceSharp.Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationParameter"/> class.
        /// </summary>
        /// <param name="evaluator">An evaluator</param>
        /// <param name="parameterName">A parameter name</param>
        /// <param name="initialValue">Initial value of parameter</param>
        public EvaluationParameter(IEvaluator evaluator, string parameterName, double initialValue)
        {
            ParameterName = parameterName ?? throw new System.ArgumentNullException(nameof(parameterName));
            Evaluator = evaluator ?? throw new System.ArgumentNullException(nameof(evaluator));
            Value = initialValue;
        }

        /// <summary>
        /// Gets the evaluator
        /// </summary>
        protected IEvaluator Evaluator { get; }

        /// <summary>
        /// Gets the parameter name
        /// </summary>
        protected string ParameterName { get; }

        /// <summary>
        /// Sets the value of paramater
        /// </summary>
        /// <param name="value">A value to set</param>
        public override void Set(double value)
        {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (Value != value)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
            {
                Evaluator.SetParameter(ParameterName, value);
            }

            base.Set(value);
        }
    }
}
