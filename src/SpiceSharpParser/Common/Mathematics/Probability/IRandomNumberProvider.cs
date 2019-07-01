namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Interface for all random number providers.
    /// </summary>
    public interface IRandomNumberProvider : IRandomIntegerProvider, IRandomDoubleProvider
    {
    }

    /// <summary>
    /// Interface for all random double providers.
    /// </summary>
    public interface IRandomDoubleProvider
    {
        /// <summary>
        /// Computes new random double in range (-1, 1).
        /// </summary>
        /// <returns>
        /// Random double in range (-1, 1).
        /// </returns>
        double NextSignedDouble();

        /// <summary>
        /// Computes new random double in range (0, 1).
        /// </summary>
        /// <returns>
        /// Random double in range (0, 1).
        /// </returns>
        double NextDouble();
    }

    /// <summary>
    /// Interface for all random integer providers.
    /// </summary>
    public interface IRandomIntegerProvider
    {
        /// <summary>
        /// Computes random integer.
        /// </summary>
        /// <returns>
        /// A new random integer.
        /// </returns>
        int Next();
    }
}
