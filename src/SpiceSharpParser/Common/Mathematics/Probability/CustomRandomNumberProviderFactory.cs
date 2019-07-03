using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Provider of random number generators.
    /// </summary>
    public class CustomRandomNumberProviderFactory : IRandomNumberProviderFactory
    {
        private static int _tickCount = Environment.TickCount;

        private readonly Pdf _pdf;
        private readonly Cdf _cdf;
        private readonly int _cdfPoints;
        private readonly Dictionary<int, IRandomNumberProvider> _randomGenerators = new Dictionary<int, IRandomNumberProvider>();
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRandomNumberProviderFactory"/> class.
        /// </summary>
        /// <param name="pdf">Pdf.</param>
        /// <param name="cdfPoints">Number of cdf points.</param>
        public CustomRandomNumberProviderFactory(Pdf pdf, int cdfPoints)
        {
            _pdf = pdf ?? throw new ArgumentNullException(nameof(pdf));
            if (cdfPoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cdfPoints));
            }

            _cdfPoints = cdfPoints;
            _cdf = new Cdf(_pdf, _cdfPoints);
        }

        /// <summary>
        /// Clears the randomizer.
        /// </summary>
        public void Clear()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _randomGenerators.Clear();
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Provides a random number generator.
        /// </summary>
        /// <param name="randomSeed">Random generator seed.</param>
        /// <returns>
        /// A new instance of a random number generator.
        /// </returns>
        public IRandomNumberProvider GetRandom(int? randomSeed)
        {
            if (randomSeed.HasValue)
            {
                _cacheLock.EnterUpgradeableReadLock();
                try
                {
                    if (!_randomGenerators.ContainsKey(randomSeed.Value))
                    {
                        _cacheLock.EnterWriteLock();
                        try
                        {
                            var randomGenerator = new CustomRandomNumberProvider(_cdf, new DefaultRandomNumberProvider(new Random(randomSeed.Value)));
                            _randomGenerators[randomSeed.Value] = randomGenerator;
                            return randomGenerator;
                        }
                        finally
                        {
                            _cacheLock.ExitWriteLock();
                        }
                    }
                    else
                    {
                        return _randomGenerators[randomSeed.Value];
                    }
                }
                finally
                {
                    _cacheLock.ExitUpgradeableReadLock();
                }
            }
            else
            {
                int seed = Interlocked.Increment(ref _tickCount);
                return new CustomRandomNumberProvider(_cdf, new DefaultRandomNumberProvider(new Random(seed)));
            }
        }

        /// <summary>
        /// Provides a random double generator.
        /// </summary>
        /// <param name="randomSeed">Random generator seed.</param>
        /// <returns>
        /// A new instance of a random double generator.
        /// </returns>
        public IRandomDoubleProvider GetRandomDouble(int? randomSeed)
        {
            return GetRandom(randomSeed);
        }

        /// <summary>
        /// Provides a random integer generator.
        /// </summary>
        /// <param name="randomSeed">Random generator seed.</param>
        /// <returns>
        /// A new instance of a random integer generator.
        /// </returns>
        public IRandomIntegerProvider GetRandomInteger(int? randomSeed)
        {
            return GetRandom(randomSeed);
        }
    }
}
