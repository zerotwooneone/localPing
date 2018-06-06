using System;
using System.Collections.Generic;
using System.Linq;
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
            var dimensionValues = GetPingResponseValues(pingResponse, GetStatusDimension, GetAddressDimension);
            return new Vector.Vector(dimensionValues);
        }

        private IEnumerable<IDimensionValue> GetPingResponseValues(IPingResponse pingResponse,
            params Func<IPingResponse, IDimensionValue>[] valuesFuncs)
        {
            return valuesFuncs.Select(f => f(pingResponse));
        }

        private DimensionValue GetStatusDimension(IPingResponse pingResponse)
        {
            var statusFlagDimension = _dimensionKeyUtil.GetStatusFlag(pingResponse.TargetIpAddress);
            var dimensionKey = _dimensionKeyFactory.GetOrCreate(statusFlagDimension);
            var intValue = pingResponse.Status.GetHashCode();
            var doubleValue = HashInt(intValue);
            var statusDimensionValue = new DimensionValue(dimensionKey, doubleValue);
            return statusDimensionValue;
        }

        private DimensionValue GetAddressDimension(IPingResponse pingResponse)
        {
            var dimensionKey = _dimensionKeyFactory.GetOrCreate("Ip Address");
            var intValue = pingResponse.TargetIpAddress.GetHashCode();
            var doubleValue = HashInt(intValue);
            var statusDimensionValue = new DimensionValue(dimensionKey, doubleValue);
            return statusDimensionValue;
        }

        private double HashInt(int value)
        {
            return (value + 7.0) * 13; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }
    }
}
