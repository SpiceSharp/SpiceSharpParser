using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Provider of random number generator.
    /// </summary>
    public class Randomizer
    {
        private static int _tickCount = Environment.TickCount;
        private Dictionary<int, Random> _randomGenerators = new Dictionary<int, Random>();
        private ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
        public Random GetRandom(int? randomSeed)
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
                            var randomGenerator = new Random(randomSeed.Value);
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
                return new Random(seed);
            }
        }
    }
}
