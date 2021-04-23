using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public class SpiceTokenProviderPool : ISpiceTokenProviderPool
    {
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly Dictionary<string, ISpiceTokenProvider> _providers = new Dictionary<string, ISpiceTokenProvider>();

        public ISpiceTokenProvider GetSpiceTokenProvider(SpiceLexerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string key = settings.HasTitle + "_" + settings.IsDotStatementNameCaseSensitive;

            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (!_providers.ContainsKey(key))
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        var provider = new SpiceTokenProvider(settings.HasTitle, settings.IsDotStatementNameCaseSensitive, settings.EnableBusSyntax);
                        _providers[key] = provider;
                        return provider;
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return _providers[key];
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}