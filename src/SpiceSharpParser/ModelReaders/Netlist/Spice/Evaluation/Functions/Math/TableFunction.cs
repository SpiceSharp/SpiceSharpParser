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

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            var parameterValue = args[0];

            var points = new List<Point>();

            for (var i = 1; i < args.Length - 1; i += 2)
            {
                var pointX = args[i];
                var pointY = args[i + 1];
                points.Add(new Point { X = pointX, Y = pointY });

                if (pointX == parameterValue)
                {
                    return pointY;
                }
            }

            points.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            if (points.Count == 1)
            {
                throw new Exception("There is only one point for table interpolation.");
            }

            // Get point + 1 line parameters for each segment of line
            LineDefinition[] linesDefinition = CreateLineParameters(points);

            int index = 0;

            while (index < points.Count && points[index].X < parameterValue)
            {
                index++;
            }

            if (index == points.Count)
            {
                return points[points.Count - 1].Y;
            }

            if (index == 0 && points[0].X > parameterValue)
            {
                return points[0].Y;
            }

            return (linesDefinition[index].A * parameterValue) + linesDefinition[index].B;
        }


        private static LineDefinition[] CreateLineParameters(List<Point> points)
        {
            List<LineDefinition> result = new List<LineDefinition>();

            for (var i = 0; i < points.Count - 1; i++)
            {
                double x1 = points[i].X;
                double x2 = points[i + 1].X;
                double y1 = points[i].Y;
                double y2 = points[i + 1].Y;

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

        public class Point
        {
            public double X { get; set; }

            public double Y { get; set; }
        }
    }
}
