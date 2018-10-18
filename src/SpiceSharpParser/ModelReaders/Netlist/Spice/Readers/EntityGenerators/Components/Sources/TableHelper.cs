using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public class TableHelper
    {
        public static string CreateTableExpression(string tableParameter, ExpressionEqualParameter eep)
        {
            var expression =
                $"table({tableParameter},{string.Join(",", eep.Points.Values.Select(v => v.X.Image + "," + v.Y.Image).ToArray())})";
            return expression;
        }
    }
}
