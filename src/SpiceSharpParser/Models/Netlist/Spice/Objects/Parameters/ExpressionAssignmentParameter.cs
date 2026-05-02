namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// An expression-to-expression assignment parameter.
    /// </summary>
    public class ExpressionAssignmentParameter : Parameter
    {
        public ExpressionAssignmentParameter(string leftExpression, string rightExpression, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
        }

        public string LeftExpression { get; }

        public string RightExpression { get; }

        public override string ToString()
        {
            return $"{{{LeftExpression}}} = {{{RightExpression}}}";
        }

        public override SpiceObject Clone()
        {
            return new ExpressionAssignmentParameter(LeftExpression, RightExpression, LineInfo);
        }
    }
}
