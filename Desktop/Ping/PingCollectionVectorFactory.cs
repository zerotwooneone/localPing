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
        
        public IVector GetVector(IEnumerable<IVector> pingVectors)
        {
            var dimensionValueArrays = pingVectors.Select(pingVector =>
            {
                var vectorDimensionValues = pingVector.DimensionValues;
                return vectorDimensionValues;
            });
            var dimensionValues = dimensionValueArrays.SelectMany(i => i);
            return new Vector.Vector(dimensionValues);
        }
    }
}