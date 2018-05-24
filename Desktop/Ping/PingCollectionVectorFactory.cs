using System.Collections.Generic;
using System.Linq;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingCollectionVectorFactory : IPingCollectionVectorFactory
    {
        private readonly PerResponseDimensionValueFactory _ipStatusDimension;

        public PingCollectionVectorFactory()
        {
            var perIpDimensionKeyFactory = new PerIpDimensionKeyFactory();

            IDimensionKey DimensionKeyFactory(IPingResponse pr)
            {
                return perIpDimensionKeyFactory.GetDimensionKey(() => $"{pr.TargetIpAddress} status", pr.TargetIpAddress);
            }

            double DimensionValueFactory(IPingResponse pr)
            {
                var status = pr.Status.GetHashCode();
                const double statusTrue = 104;
                const double statusFalse = 117;
                var statusFlag = status == 0 ? statusTrue : statusFalse;
                var dimValue = statusFlag;
                return dimValue;
            }

            _ipStatusDimension = new PerResponseDimensionValueFactory(DimensionKeyFactory,
                DimensionValueFactory);
        }

        public IVector GeVector(IEnumerable<IPingResponse> pingResponses)
        {
            return new Vector.Vector(pingResponses.Select(response =>
            {
                var dimensionValue = _ipStatusDimension.GetDimensionValue(response);
                return dimensionValue;
            }));
        }
    }
}