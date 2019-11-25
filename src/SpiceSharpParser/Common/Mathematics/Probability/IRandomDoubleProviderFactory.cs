namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public interface IRandomDoubleProviderFactory
    {
        IRandomDoubleProvider GetRandomDouble(int? randomSeed);
    }
}
