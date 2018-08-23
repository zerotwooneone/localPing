using System.Collections.Concurrent;
using System.Collections.Generic;
using zh.Vector;

namespace zh.LocalPingLib.Ping
{
    public class DimensionKeyFactory : IDimensionKeyFactory
    {
        private readonly IDictionary<string, IDimensionKey> _dimensionKeys;
        private static readonly object SyncLock = new object();

        public DimensionKeyFactory()
        {
            _dimensionKeys = new ConcurrentDictionary<string, IDimensionKey>();
        }

        public IDimensionKey GetOrCreate(string name)
        {
            IDimensionKey dimensionKey;
            if (_dimensionKeys.TryGetValue(name, out IDimensionKey found))
            {
                dimensionKey = found;
            }
            else
            {
                lock (SyncLock)
                {
                    if (_dimensionKeys.TryGetValue(name, out IDimensionKey found2))
                    {
                        dimensionKey = found2;
                    }
                    else
                    {
                        dimensionKey = new DimensionKey(name);
                        _dimensionKeys[name] = dimensionKey;
                    }
                }

            }

            return dimensionKey;
        }
    }
}