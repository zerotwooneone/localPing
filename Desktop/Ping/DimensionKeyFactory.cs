using System.Collections.Concurrent;
using System.Collections.Generic;
using Desktop.Vector;

namespace Desktop.Ping
{
    public class DimensionKeyFactory : IDimensionKeyFactory
    {
        private readonly IDictionary<string, IDimensionKey> _dimensionKeys;

        public DimensionKeyFactory()
        {
            _dimensionKeys = new ConcurrentDictionary<string, IDimensionKey>();
        }

        public IDimensionKey GetOrCreate(string name)
        {
            IDimensionKey dimensionKey;
            if (_dimensionKeys.TryGetValue(name, out var found))
            {
                dimensionKey = found;
            }
            else
            {
                dimensionKey = new DimensionKey(name);
                _dimensionKeys[name] = dimensionKey;
            }

            return dimensionKey;
        }
    }
}