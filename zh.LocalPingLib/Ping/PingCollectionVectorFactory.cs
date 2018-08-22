using System.Collections.Generic;
using System.Linq;
using Desktop.Vector;

namespace zh.LocalPingLib.Ping
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
            return new Desktop.Vector.Vector(dimensionValues);
        }
    }
}