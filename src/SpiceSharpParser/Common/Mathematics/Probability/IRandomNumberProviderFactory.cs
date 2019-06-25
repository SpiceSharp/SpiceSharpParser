namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public interface IRandomDoubleProviderFactory
    {
        IRandomDoubleProvider GetRandomDouble(int? randomSeed);
    }

    public interface IRandomIntegerProviderFactory
    {
        IRandomIntegerProvider GetRandomInteger(int? randomSeed);
    }

    public interface IRandomNumberProviderFactory : IRandomDoubleProviderFactory, IRandomIntegerProviderFactory
    {
    }
}
