namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// An parameter that triggers re-evaluation when changed
    /// </summary>
    public class EvaluationParameter : SpiceSharp.GivenParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationParameter"/> class.
        /// </summary>
        /// <param name="evaluator">An evaluator</param>
        /// <param name="parameterName">A parameter name</param>
        public EvaluationParameter(IEvaluator evaluator, string parameterName)
        {
            ParameterName = parameterName ?? throw new System.ArgumentNullException(nameof(parameterName));
            Evaluator = evaluator ?? throw new System.ArgumentNullException(nameof(evaluator));        }

        /// <summary>
        /// Gets or sets the value of parameter
        /// </summary>
        public override double Value
        {
            get
            {
                return base.Value;
            }

            set
            {
                Evaluator.SetParameter(ParameterName, value);
                base.Value = value;
            }
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
        /// Clones the parameter
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new EvaluationParameter(Evaluator, ParameterName);
        }
    }
}
