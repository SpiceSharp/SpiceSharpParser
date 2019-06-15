using SpiceSharpParser.Common.Evaluation;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class TableFunction : Function<double, double>
    {
        public TableFunction()
        {
            Name = "table";
            ArgumentsCount = -1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            var parameterValue = (double)args[0];

            var points = new List<Tuple<double, double>>();

            for (var i = 1; i < args.Length - 1; i += 2)
            {
                var pointX = (double)args[i];
                var pointY = (double)args[i + 1];
                points.Add(new Tuple<double, double>(pointX, pointY));

                if (pointX == parameterValue)
                {
                    return pointY;
                }
            }

            points.Sort((x1, x2) => x1.Item1.CompareTo(x2.Item1));
            if (points.Count == 1)
            {
                throw new Exception("There is only one point for table interpolation.");
            }

            // Get point + 1 line parameters for each segment of line
            LineDefinition[] linesDefinition = CreateLineParameters(points);

            int index = 0;

            while (index < points.Count && points[index].Item1 < parameterValue)
            {
                index++;
            }

            if (index == points.Count)
            {
                return points[points.Count - 1].Item2;
            }

            if (index == 0 && points[0].Item1 > parameterValue)
            {
                return points[0].Item2;
            }

            return (linesDefinition[index].A * parameterValue) + linesDefinition[index].B;
        }


        private static LineDefinition[] CreateLineParameters(List<Tuple<double, double>> points)
        {
            List<LineDefinition> result = new List<LineDefinition>();

            for (var i = 0; i < points.Count - 1; i++)
            {
                double x1 = points[i].Item1;
                double x2 = points[i + 1].Item1;
                double y1 = points[i].Item2;
                double y2 = points[i + 1].Item2;

                double a = (y2 - y1) / (x2 - x1);

                result.Add(new LineDefinition()
                {
                    A = a,
                    B = y1 - (a * x1),
                });
            }

            result.Insert(0, result[0]);
            result.Add(result[result.Count - 1]);
            return result.ToArray();
        }

        public class LineDefinition
        {
            public double A { get; set; }

            public double B { get; set; }
        }
    }
}
