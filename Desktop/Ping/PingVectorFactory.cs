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
        private readonly IPingResponseUtil _pingResponseUtil;
        private readonly PingStatsUtil _pingStatsUtil;

        public PingVectorFactory(IDimensionKeyFactory dimensionKeyFactory,
            IPingResponseUtil pingResponseUtil,
            PingStatsUtil pingStatsUtil)
        {
            _dimensionKeyFactory = dimensionKeyFactory;
            _pingResponseUtil = pingResponseUtil;
            _pingStatsUtil = pingStatsUtil;
        }
        public IVector GetVector(IPingResponse pingResponse, IPingStats stats)
        {
            var pingResponseValues = GetPingResponseValues(pingResponse, GetStatusDimension);
            var pingResponseValueX = GetPingResponseValuesX(pingResponse,GetAddressDimensions, r=>GetStatsValue(r.TargetIpAddress, stats), pr=>GetRttDimension(pr.RoundTripTime));
            var dimensionValues = pingResponseValues.Concat(pingResponseValueX);
            return new Vector.Vector(dimensionValues);
        }

        private IEnumerable<IDimensionValue> GetRttDimension(TimeSpan roundTripTime)
        {
            var rtt = Hash(roundTripTime.TotalMilliseconds);
            var rttValueDimensionName = _dimensionKeyFactory.GetOrCreate($"Round Trip Time {roundTripTime.ToString()}");
            return new[]
            {
                new DimensionValue(rttValueDimensionName, rtt),
            };
        }

        private IEnumerable<IDimensionValue> GetStatsValue(IPAddress pingResponseTargetIpAddress, IPingStats stats)
        {
            var averageSuccessRate = Hash(_pingStatsUtil.GetAverageSuccessRate(stats.StatusHistory));
            var scopedAverageDimensionName = _dimensionKeyFactory.GetOrCreate($"Average Success Rate {averageSuccessRate}");
            var avg25ValueName = _dimensionKeyFactory.GetOrCreate($"Average 25 {stats.Average25}");
            var avg25Value = Hash(stats.Average25)*1000;
            var lastSuccessDimensionName = _dimensionKeyFactory.GetOrCreate($"Last Success {stats.LastSuccess}");
            var lastFailureDimensionName = _dimensionKeyFactory.GetOrCreate($"Last Failure {stats.LastFailure}");
            return new[] {
                new DimensionValue(scopedAverageDimensionName, averageSuccessRate),
                new DimensionValue(avg25ValueName, avg25Value),
                new DimensionValue(lastSuccessDimensionName, Hash(stats.LastSuccess)),
                new DimensionValue(lastFailureDimensionName, Hash(stats.LastFailure)),
            };
        }

        private double Hash(DateTime timeStamp)
        {
            var diff = DateTime.Now - timeStamp;
            var longValue = diff.Ticks;
            var doubleValue = (double) longValue;
            return Hash(doubleValue);
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
            const double success = (1.0 + 7) * 13;
            const double failure = (0.0 + 7) * 13;
            var isSuccess = _pingResponseUtil.IsSuccess(pingResponse.Status);
            var statusFlagValue = isSuccess ? success : failure;
            var dimensionKey = _dimensionKeyFactory.GetOrCreate($"status flag {isSuccess}");
            var statusDimensionValue = new DimensionValue(dimensionKey, statusFlagValue);
            return statusDimensionValue;
        }

        private IEnumerable<IDimensionValue> GetAddressDimensions(IPingResponse pingResponse)
        {
            var intValue = pingResponse.TargetIpAddress.GetHashCode();
            var ipValue = Hash(intValue);
            
            var ipDimensionKey = _dimensionKeyFactory.GetOrCreate($"Ip Address {pingResponse.TargetIpAddress}");
            var ipSpecDimensionValue = new DimensionValue(ipDimensionKey, ipValue);

            return new[]
            {
                ipSpecDimensionValue
            };
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
