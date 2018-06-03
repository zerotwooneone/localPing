using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingVectorFactory : IPingVectorFactory
    {
        private readonly IDimensionKeyFactory _dimensionKeyFactory;
        private readonly IDimensionKeyUtil _dimensionKeyUtil;

        public PingVectorFactory(IDimensionKeyFactory dimensionKeyFactory,
            IDimensionKeyUtil dimensionKeyUtil)
        {
            _dimensionKeyFactory = dimensionKeyFactory;
            _dimensionKeyUtil = dimensionKeyUtil;
        }
        public IVector GetVector(IPingResponse pingResponse)
        {
            var statusFlagDimension = _dimensionKeyUtil.GetStatusFlag(pingResponse.TargetIpAddress);
            var dimensionKey = _dimensionKeyFactory.GetOrCreate(statusFlagDimension);
            return new Vector.Vector(new[] { new DimensionValue(dimensionKey, pingResponse.Status.GetHashCode()) });
        }
    }
}
