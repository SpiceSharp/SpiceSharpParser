namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public interface IRandomIntegerProviderFactory
    {
        IRandomIntegerProvider GetRandomInteger(int? randomSeed);
    }
}
