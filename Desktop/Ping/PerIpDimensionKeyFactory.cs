using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Desktop.Vector;

namespace Desktop.Ping
{
    public class PerIpDimensionKeyFactory 
    {
        private readonly IDictionary<IPAddress, IDimensionKey> _dimensionKeys;
        
        public PerIpDimensionKeyFactory()
        {
            _dimensionKeys = new ConcurrentDictionary<IPAddress, IDimensionKey>();
        }

        public IDimensionKey GetDimensionKey(Func<string> dimensionNameFactory, IPAddress ipAddress)
        {
            IDimensionKey dimensionKey;
            if (_dimensionKeys.TryGetValue(ipAddress, out var found))
            {
                dimensionKey = found;
            }
            else
            {
                var name = dimensionNameFactory();
                dimensionKey = new DimensionKey(name);
                _dimensionKeys[ipAddress] = dimensionKey;
            }

            return dimensionKey;
        }
    }
}