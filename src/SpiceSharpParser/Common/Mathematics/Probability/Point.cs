namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public Point Clone()
        {
            return new Point(X, Y);
        }
    }
}