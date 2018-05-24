using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingCollectionVectorFactory : IPingCollectionVectorFactory
    {
        private readonly IDictionary<IPAddress, DimensionKey> _ipStatusDimensionKeys;

        public PingCollectionVectorFactory()
        {
            _ipStatusDimensionKeys = new ConcurrentDictionary<IPAddress, DimensionKey>();
        }

        public IVector GeVector(IEnumerable<IPingResponse> pingResponses)
        {
            return new Vector.Vector(pingResponses.Select(response =>
            {
                DimensionKey dimensionKey;
                if (_ipStatusDimensionKeys.TryGetValue(response.TargetIpAddress, out var key))
                {
                    dimensionKey = key;
                }
                else
                {
                    dimensionKey = new DimensionKey($"{response.TargetIpAddress} status");
                    _ipStatusDimensionKeys[response.TargetIpAddress] = dimensionKey;
                }

                var status = response.Status.GetHashCode();
                const double statusTrue=104;
                const double statusFalse = 117;
                var statusFlag = status == 0 ? statusTrue : statusFalse;
                var dimValue = statusFlag;
                return new DimensionValue(dimensionKey, dimValue);
            }));
        }
    }
}