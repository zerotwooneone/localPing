using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingVectorFactory : IPingVectorFactory
    {
        private readonly IDimensionKeyFactory _dimensionKeyFactory;
        private readonly IDimensionKeyUtil _dimensionKeyUtil;
        private readonly IPingResponseUtil _pingResponseUtil;
        private readonly PingStatsUtil _pingStatsUtil;

        public PingVectorFactory(IDimensionKeyFactory dimensionKeyFactory,
            IDimensionKeyUtil dimensionKeyUtil,
            IPingResponseUtil pingResponseUtil,
            PingStatsUtil pingStatsUtil)
        {
            _dimensionKeyFactory = dimensionKeyFactory;
            _dimensionKeyUtil = dimensionKeyUtil;
            _pingResponseUtil = pingResponseUtil;
            _pingStatsUtil = pingStatsUtil;
        }
        public IVector GetVector(IPingResponse pingResponse, IPingStats stats)
        {
            var pingResponseValues = GetPingResponseValues(pingResponse, GetStatusDimension, GetAddressDimension);
            var statValues = GetStatsValue(pingResponse.TargetIpAddress, stats);
            var dimensionValues = pingResponseValues.Concat(statValues);
            return new Vector.Vector(dimensionValues);
        }

        private IEnumerable<IDimensionValue> GetStatsValue(IPAddress pingResponseTargetIpAddress, IPingStats stats)
        {
            var averageSuccessRate = Hash(_pingStatsUtil.GetAverageSuccessRate(stats.StatusHistory));
            var averageDimensionName = _dimensionKeyFactory.GetOrCreate("Average Success Rate");
            var scopedAverageDimensionName = _dimensionKeyFactory.GetOrCreate($"Average Success Rate {averageSuccessRate}");
            return new[] { new DimensionValue(averageDimensionName, averageSuccessRate), new DimensionValue(scopedAverageDimensionName, averageSuccessRate) };
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
            const double success = (1.0 + 7) * 13;
            const double failure = (0.0 + 7) * 13;
            var statusFlagValue = _pingResponseUtil.IsSuccess(pingResponse.Status) ? success : failure;
            var statusDimensionValue = new DimensionValue(dimensionKey, statusFlagValue);
            return statusDimensionValue;
        }

        private DimensionValue GetAddressDimension(IPingResponse pingResponse)
        {
            var dimensionKey = _dimensionKeyFactory.GetOrCreate("Ip Address");
            var intValue = pingResponse.TargetIpAddress.GetHashCode();
            var ipValue = Hash(intValue) % 1000;
            var ipDimensionValue = new DimensionValue(dimensionKey, ipValue);
            return ipDimensionValue;
        }

        private double Hash(int value)
        {
            return (value + 7.0) * 13; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }

        private double Hash(double value)
        {
            return (value + 7.0) * 13; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }
    }
}
