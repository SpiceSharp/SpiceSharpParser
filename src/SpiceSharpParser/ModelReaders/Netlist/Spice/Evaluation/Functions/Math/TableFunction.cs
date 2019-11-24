using System;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class TableFunction : Function<double, double>, IDerivativeFunction<double, double>
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

            return (linesDefinition[index].A * parameterValue) + linesDefinition[index].B;
        }

        public Derivatives<Func<double>> Derivative(string image, FunctionFoundEventArgs<Derivatives<Func<double>>> args, EvaluationContext context)
        {
            return GetDerivatives(args);
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

            result.Insert(0, new LineDefinition() { A = 0, B = points[0].Y });
            result.Add(new LineDefinition() { A = 0, B = points[points.Count - 1].Y });
            return result.ToArray();
        }

        private static Derivatives<Func<double>> GetDerivatives(FunctionFoundEventArgs<Derivatives<Func<double>>> args)
        {
            Derivatives<Func<double>> derivatives = new DoubleDerivatives(2);

            var parameterValue = args[0][0];
            var points = new List<Point>();
            var value = parameterValue();

            for (var i = 1; i < args.ArgumentCount - 1; i += 2)
            {
                var pointX = args[i][0]();
                var pointY = args[i + 1][0]();

                points.Add(new Point() { X = pointX, Y = pointY });

                if (pointX == value)
                {
                    derivatives[0] = () => pointY;
                    return derivatives;
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

            while (index < points.Count && points[index].X < value)
            {
                index++;
            }

            derivatives[0] = () => (linesDefinition[index].A * value) + linesDefinition[index].B;
            derivatives[1] = () =>
            {
                double result = 0;
                for (var i = 0; i < args[0].Count; i++)
                {
                    if (args[0][i + 1] != null)
                    {
                        result += linesDefinition[index].A * args[0][i + 1]();
                    }
                }

                return result;
            };
            return derivatives;
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