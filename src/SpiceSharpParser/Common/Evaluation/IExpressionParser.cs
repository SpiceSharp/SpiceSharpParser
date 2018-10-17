namespace SpiceSharpParser.Common.Evaluation
{
    public interface IExpressionParser
    {
        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The parser context.</param>
        /// <returns>Returns the result of parsing.</returns>
        ExpressionParseResult Parse(string expression, ExpressionParserContext context);
    }
}
