namespace SpiceSharpParser.ModelReader.Spice.Evaluation
{
    /// <summary>
    /// An parameter that triggers re-evaluation when changed
    /// </summary>
    public class EvaluationParameter : SpiceSharp.Parameter
    {
        private double rawValue;

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
                return rawValue;
            }

            set
            {
                Evaluator.SetParameter(ParameterName, value);
                rawValue = value;
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
        /// <returns>
        /// A clone of parameter
        /// </returns>
        public override SpiceSharp.Parameter Clone()
        {
            return new EvaluationParameter(Evaluator, ParameterName);
        }
    }
}
