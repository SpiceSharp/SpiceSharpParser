using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    using SpiceSharpParser.Parsers.Expression;

    /// <summary>
    /// Spice expressions evaluator.
    /// </summary>
    public class SpiceEvaluator : Evaluator
    {
        public SpiceEvaluator()
            : this(string.Empty, new SpiceExpressionParser(), false, false)
        {
        }

        public SpiceEvaluator(
            string name,
            IExpressionParser parser,
            bool isParameterNameCaseSensitive,
            bool isFunctionNameCaseSensitive)
            : base(name, parser, isParameterNameCaseSensitive, isFunctionNameCaseSensitive)
        {
        }
    }
}
