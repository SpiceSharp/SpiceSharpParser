namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// An interface for all evaluators
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Evalues a specific string to double
        /// </summary>
        /// <param name="expression">An expression to evaluate</param>
        /// <returns>
        /// A double value
        /// </returns>
        double EvaluteDouble(string expression);

        /// <summary>
        /// Sets the parameter value and updates the values expressions
        /// </summary>
        /// <param name="parameterName">A name of parameter</param>
        /// <param name="value">A value of parameter</param>
        void SetParameter(string parameterName, double value);

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change
        /// </summary>
        /// <param name="expression">An expression to add</param>
        void AddDynamicExpression(DoubleExpression expression);
    }
}
