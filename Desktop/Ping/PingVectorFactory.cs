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
            var pingResponseValues = GetPingResponseValues(pingResponse, GetStatusDimension);
            var pingResponseValueX = GetPingResponseValuesX(pingResponse,GetAddressDimensions, r=>GetStatsValue(r.TargetIpAddress, stats));
            //var statValues = GetStatsValue(pingResponse.TargetIpAddress, stats);
            //var addressValues = GetAddressDimensions(pingResponse);
            var dimensionValues = pingResponseValues.Concat(pingResponseValueX);
            return new Vector.Vector(dimensionValues);
        }

        private IEnumerable<IDimensionValue> GetStatsValue(IPAddress pingResponseTargetIpAddress, IPingStats stats)
        {
            var averageSuccessRate = Hash(_pingStatsUtil.GetAverageSuccessRate(stats.StatusHistory));
            var averageDimensionName = _dimensionKeyFactory.GetOrCreate("Average Success Rate");
            var scopedAverageDimensionName = _dimensionKeyFactory.GetOrCreate($"Average Success Rate {averageSuccessRate}");
            var avg25Name = _dimensionKeyFactory.GetOrCreate("Average 25");
            var avg25ValueName = _dimensionKeyFactory.GetOrCreate($"Average 25 {stats.Average25}");
            var avg25Value = Hash(stats.Average25)*1000;
            return new[] { new DimensionValue(averageDimensionName, averageSuccessRate), 
                new DimensionValue(scopedAverageDimensionName, averageSuccessRate),
                new DimensionValue(avg25Name, avg25Value),
                new DimensionValue(avg25ValueName, avg25Value)
            };
        }

        private IEnumerable<IDimensionValue> GetPingResponseValues(IPingResponse pingResponse,
            params Func<IPingResponse, IDimensionValue>[] valuesFuncs)
        {
            return valuesFuncs.Select(f => f(pingResponse));
        }

        private IEnumerable<IDimensionValue> GetPingResponseValuesX(IPingResponse pingResponse,
            params Func<IPingResponse, IEnumerable<IDimensionValue>>[] valuesFuncs)
        {
            return valuesFuncs.Select(f => f(pingResponse)).SelectMany(f=>f);
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

        private IEnumerable<IDimensionValue> GetAddressDimensions(IPingResponse pingResponse)
        {
            var dimensionKey = _dimensionKeyFactory.GetOrCreate("Ip Address");
            var intValue = pingResponse.TargetIpAddress.GetHashCode();
            var ipValue = Hash(intValue);
            var ipDimensionValue = new DimensionValue(dimensionKey, ipValue);

            var ipDimensionKey = _dimensionKeyFactory.GetOrCreate($"Ip Address {pingResponse.TargetIpAddress}");
            var ipSpecDimensionValue = new DimensionValue(ipDimensionKey, ipValue);

            return new[] { ipDimensionValue, ipSpecDimensionValue };
        }
        private const double Modulus = 1000;
        private double Hash(int value)
        {
            return ((value + 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }

        private double Hash(double value)
        {
            return ((value + 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }
    }
}
