using System;
using System.Collections.Generic;
using System.Linq;
using zh.Vector;

namespace zh.LocalPingLib.Ping
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
            //var pingResponseValues = GetPingResponseValues(pingResponse, GetStatusDimension);
            var pingResponseValueX = GetPingResponseValuesX(pingResponse,  r => GetStatsValue(r, stats)); //GetAddressDimensions,, pr => GetRttDimension(pr.RoundTripTime)
            var dimensionValues = pingResponseValueX;
            return new Vector.Vector(dimensionValues);
        }

        //private IEnumerable<IDimensionValue> GetRttDimension(TimeSpan roundTripTime)
        //{
        //    var rtt = Hash(roundTripTime.TotalMilliseconds);
        //    var rttValueDimensionName = _dimensionKeyFactory.GetOrCreate($"Round Trip Time {roundTripTime.ToString()}");
        //    return new[]
        //    {
        //        new DimensionValue(rttValueDimensionName, rtt),
        //    };
        //}

        private IEnumerable<IDimensionValue> GetStatsValue(IPingResponse pingResponse, IPingStats stats)
        {
            //var statsObj = _pingStatsUtil.GetAverageSuccessRate(stats.StatusHistory);

            //var success = _pingResponseUtil.IsSuccess(pingResponse.Status);
            //var successVal = success ? 1.0 : 0;

            //var aboveAverage = successVal > statsObj.Average;
            //var greaterThanAverage = new DimensionValue(_dimensionKeyFactory.GetOrCreate("Above Average"), aboveAverage ? Hash(true) : 0);
            
            //var halfStdDev = statsObj.StdDev / 2;
            //var avgPlusHalf = statsObj.Average + halfStdDev;
            //var aboveAvgPlusHalf = successVal > avgPlusHalf;

            //var greaterThanAveragePlusHalf = new DimensionValue(_dimensionKeyFactory.GetOrCreate("Above Average Plus Half Std Dev"), aboveAvgPlusHalf ? Hash(true) : 0);
            
            //var avgMinusHalf = statsObj.Average - halfStdDev;
            //var aboveAvgMinusHalf = successVal > avgMinusHalf;

            //var greaterThanAverageMinusHalf = new DimensionValue(_dimensionKeyFactory.GetOrCreate("Above Average Minus Half Std Dev"), aboveAvgMinusHalf ? Hash(true) : 0);

            var nullableStatus = stats.StatusHistory.Cast<bool?>();
            var statusHistoryDimensionValues = GetStatusHistoryDimensionValues(nullableStatus);
            return statusHistoryDimensionValues;
        }

        private IEnumerable<IDimensionValue> GetStatusHistoryDimensionValues(
            IEnumerable<bool?> nullableStatus)
        {
            var statusSuccesses = nullableStatus
                .Select(nb => nb ?? true);
            var successes = statusSuccesses as bool[] ?? statusSuccesses.ToArray();
            var successCount = successes.Count(b => b);
            var failureCount = successes.Count(b => !b);
            //var successPct = successCount / successes.Length;
            //var failurePct = 1 - successPct;
            //successPct *= 100;
            //failurePct *= 100;
            var is100PercentSuccess = Hash(successCount == successes.Length);
            var is100PercentFailure = Hash(failureCount == successes.Length);
            return new[]
            {
                new DimensionValue(_dimensionKeyFactory.GetOrCreate("is100PercentSuccess"), is100PercentSuccess),
                new DimensionValue(_dimensionKeyFactory.GetOrCreate("is100PercentFailure"), is100PercentFailure),
            };
        }

        private IEnumerable<IDimensionValue> StatusHistoryDimensionValues(int index, bool lastStatus)
        {
            var lastSuccess = Hash(lastStatus);
            var lastSuccessDimensionName = _dimensionKeyFactory.GetOrCreate($"Last Success {index} {lastSuccess}");
            var lsDimensionValue = new DimensionValue(lastSuccessDimensionName, Hash(lastSuccess));
            var lastFailure = Hash(!lastStatus);
            var lastFailureDimensionName = _dimensionKeyFactory.GetOrCreate($"Last Failure {index} {lastFailure}");
            var lfDimensionValue = new DimensionValue(lastFailureDimensionName, Hash(lastFailure));
            return new[] { lsDimensionValue, lfDimensionValue };
        }

        private double Hash(DateTime timeStamp)
        {
            var diff = DateTime.Now - timeStamp;
            var longValue = diff.Ticks;
            var doubleValue = (double)longValue;
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
            return valuesFuncs.Select(f => f(pingResponse)).SelectMany(f => f);
        }

        private DimensionValue GetStatusDimension(IPingResponse pingResponse)
        {
            var isSuccess = _pingResponseUtil.IsSuccess(pingResponse.Status);
            var statusFlagValue = Hash(isSuccess);
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

        private double Hash(bool value)
        {
            const double success = (1.0 + 7) * 13;
            const double failure = (0.0 + 7) * 13;
            var result = value ? success : failure;
            return result;
        }
    }
}
