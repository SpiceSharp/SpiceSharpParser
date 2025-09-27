using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Provider of random number generators.
    /// </summary>
    public class DefaultRandomNumberProviderFactory : IRandomNumberProviderFactory
    {
        private static int _tickCount = Environment.TickCount;
        private readonly Dictionary<int, IRandomNumberProvider> _randomGenerators = new Dictionary<int, IRandomNumberProvider>();
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
                            // Double-check after entering write lock
                            if (!_randomGenerators.ContainsKey(randomSeed.Value))
                            {
                                var randomGenerator = new DefaultRandomNumberProvider(new Random(randomSeed.Value));
                                _randomGenerators[randomSeed.Value] = randomGenerator;
                                return randomGenerator;
                            }
                            else
                            {
                                return _randomGenerators[randomSeed.Value];
                            }
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
                return new DefaultRandomNumberProvider(new Random(seed));
            }
        }

        public IRandomDoubleProvider GetRandomDouble(int? randomSeed)
        {
            return GetRandom(randomSeed);
        }

        public IRandomIntegerProvider GetRandomInteger(int? randomSeed)
        {
            return GetRandom(randomSeed);
        }
    }
}