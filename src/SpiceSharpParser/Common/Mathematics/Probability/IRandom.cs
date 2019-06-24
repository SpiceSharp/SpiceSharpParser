namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public interface IRandom : IRandomInteger, IRandomDouble
    {
    }

    public interface IRandomDouble
    {
        double NextDouble();
    }

    public interface IRandomInteger
    {
        int Next();
    }
}
