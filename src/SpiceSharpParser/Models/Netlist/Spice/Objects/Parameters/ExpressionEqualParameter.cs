using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class ExpressionEqualParameter : Parameter
    {
        public ExpressionEqualParameter(string expression, Points points, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Expression = expression;
            Points = points;
        }

        public string Expression { get; }

        public Points Points { get; }

        public override string ToString()
        {
            return $"{Expression} = ({string.Join(",", Points.Select(p => p.ToString()))})";
        }

        public override SpiceObject Clone()
        {
            var result = new ExpressionEqualParameter(Expression, (Points)Points.Clone(), LineInfo);
            return result;
        }
    }
}