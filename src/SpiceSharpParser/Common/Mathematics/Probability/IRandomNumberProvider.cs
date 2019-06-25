namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public interface IRandomNumberProvider : IRandomIntegerProvider, IRandomDoubleProvider
    {
    }

    public interface IRandomDoubleProvider
    {
        double NextDouble();
    }

    public interface IRandomIntegerProvider
    {
        int Next();
    }}
