using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingCollectionVectorFactory : IPingCollectionVectorFactory
    {
        private readonly IPingVectorFactory _pingVectorFactory;
        private IReadOnlyDictionary<IPAddress, IReadOnlyDictionary<IDimensionKey, Func<IPingResponse, Task<double>>>> _ipSpecificDimensions;

        public PingCollectionVectorFactory(IPingVectorFactory pingVectorFactory)
        {
            _pingVectorFactory = pingVectorFactory;
        }

        public IVector GetVector(IEnumerable<IPingResponse> pingResponses)
        {
            var dimensionValueArrays = pingResponses.Select(response =>
            {
                var pingVector = _pingVectorFactory.GetVector(response);
                var vectorDimensionValues = pingVector.DimensionValues;
                return vectorDimensionValues;
            });
            var dimensionValues = dimensionValueArrays.SelectMany(i => i);
            return new Vector.Vector(dimensionValues);
        }
    }
}