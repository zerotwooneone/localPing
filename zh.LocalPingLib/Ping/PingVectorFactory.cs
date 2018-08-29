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
            IEnumerable<IDimensionValue> pingResponseValueX = GetPingResponseValuesX(pingResponse, r => GetStatsValue(r, stats)); //GetAddressDimensions,, pr => GetRttDimension(pr.RoundTripTime)
            IEnumerable<IDimensionValue> dimensionValues = pingResponseValueX;
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

            IEnumerable<bool?> nullableStatus = stats.StatusHistory.Cast<bool?>();
            IEnumerable<IDimensionValue> statusHistoryDimensionValues = GetStatusHistoryDimensionValues(nullableStatus);

            DateTime? statsLastSuccess = stats.LastSuccess;
            DateTime? statsLastFailure = stats.LastFailure;

            IEnumerable<IDimensionValue> lastDims = LastDimensions(statsLastSuccess, statsLastFailure);

            return statusHistoryDimensionValues.Concat(lastDims);
        }

        private IEnumerable<IDimensionValue> LastDimensions(DateTime? statsLastSuccess, DateTime? statsLastFailure)
        {
            TimeSpan fiveMinutes = TimeSpan.FromMinutes(5);
            TimeSpan oneMinute = TimeSpan.FromMinutes(1);
            
            var last5Dim = LastDimensions(statsLastSuccess, statsLastFailure, fiveMinutes, DimensionNames.FiveMinuteDeltaSinceLastSuccessOrFailure);
            var last1Dim = LastDimensions(statsLastSuccess, statsLastFailure, oneMinute, DimensionNames.OneMinuteDeltaSinceLastSuccessOrFailure);
            var lastDims = new IDimensionValue[]
            {
                last5Dim,
                last1Dim
            };
            return lastDims;
        }

        private DimensionValue LastDimensions(DateTime? statsLastSuccess, DateTime? statsLastFailure, TimeSpan fiveMinutes,
            string dimensionName)
        {
            DateTime? lastSuccessIn5Min = statsLastSuccess.HasValue && (DateTime.Now - statsLastSuccess) < fiveMinutes
                ? statsLastSuccess
                : null;

            DateTime? lastFailureIn5Min = statsLastFailure.HasValue && (DateTime.Now - statsLastFailure) < fiveMinutes
                ? statsLastFailure
                : null;
            double last5Value;
            if (lastSuccessIn5Min.HasValue && lastFailureIn5Min.HasValue)
            {
                last5Value = 0.0; //(lastSuccessIn5Min.Value - lastFailureIn5Min.Value).Ticks;
            }
            else
            {
                last5Value = 1000;
            }

            double fiveMinuteDeltaSinceLastSuccessOrFailure = Hash(last5Value);

            DimensionValue last5Dim =
                new DimensionValue(
                    _dimensionKeyFactory.GetOrCreate(dimensionName),
                    fiveMinuteDeltaSinceLastSuccessOrFailure);
            return last5Dim;
        }

        private IEnumerable<IDimensionValue> GetStatusHistoryDimensionValues(
            IEnumerable<bool?> nullableStatus)
        {
            IEnumerable<bool> statusSuccesses = nullableStatus
                .Select(nb => nb ?? true);
            bool[] successes = statusSuccesses as bool[] ?? statusSuccesses.ToArray();
            int successCount = successes.Count(b => b);
            int failureCount = successes.Count(b => !b);
            //var successPct = successCount / successes.Length;
            //var failurePct = 1 - successPct;
            //successPct *= 100;
            //failurePct *= 100;
            double allSuccessOrFailure = Hash(successCount == successes.Length || failureCount == successes.Length);
            double has1SuccessOrFailure = Hash(successCount == 1 || failureCount == 1);

            return new[]
            {
                new DimensionValue(_dimensionKeyFactory.GetOrCreate(DimensionNames.Is100PercentSuccessOrFailure), allSuccessOrFailure),
                new DimensionValue(_dimensionKeyFactory.GetOrCreate(DimensionNames.Has1SuccessOrFailure), has1SuccessOrFailure),
            };
        }

        private IEnumerable<IDimensionValue> StatusHistoryDimensionValues(int index, bool lastStatus)
        {
            double lastSuccess = Hash(lastStatus);
            IDimensionKey lastSuccessDimensionName = _dimensionKeyFactory.GetOrCreate($"Last Success {index} {lastSuccess}");
            DimensionValue lsDimensionValue = new DimensionValue(lastSuccessDimensionName, Hash(lastSuccess));
            double lastFailure = Hash(!lastStatus);
            IDimensionKey lastFailureDimensionName = _dimensionKeyFactory.GetOrCreate($"Last Failure {index} {lastFailure}");
            DimensionValue lfDimensionValue = new DimensionValue(lastFailureDimensionName, Hash(lastFailure));
            return new[] { lsDimensionValue, lfDimensionValue };
        }

        private double Hash(DateTime timeStamp)
        {
            TimeSpan diff = DateTime.Now - timeStamp;
            long longValue = diff.Ticks;
            double doubleValue = (double)longValue;
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
            bool isSuccess = _pingResponseUtil.IsSuccess(pingResponse.Status);
            double statusFlagValue = Hash(isSuccess);
            IDimensionKey dimensionKey = _dimensionKeyFactory.GetOrCreate($"status flag {isSuccess}");
            DimensionValue statusDimensionValue = new DimensionValue(dimensionKey, statusFlagValue);
            return statusDimensionValue;
        }

        private IEnumerable<IDimensionValue> GetAddressDimensions(IPingResponse pingResponse)
        {
            int intValue = pingResponse.TargetIpAddress.GetHashCode();
            double ipValue = Hash(intValue);

            IDimensionKey ipDimensionKey = _dimensionKeyFactory.GetOrCreate($"Ip Address {pingResponse.TargetIpAddress}");
            DimensionValue ipSpecDimensionValue = new DimensionValue(ipDimensionKey, ipValue);

            return new[]
            {
                ipSpecDimensionValue
            };
        }
        private const double Modulus = 1000;
        public static double Hash(int value)
        {
            if (value >= 0)
            {
                return ((value + 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
            }
            else
            {
                return ((value - 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
            }
        }

        public static double Hash(double value)
        {
            //if (double.IsPositiveInfinity(value))
            //{
            //    return Double.PositiveInfinity;
            //}
            //if (double.IsNegativeInfinity(value))
            //{
            //    return Double.NegativeInfinity;
            //}
            if (value >= 0)
            {
                return ((value + 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
            }
            else
            {
                return ((value - 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
            }

        }

        public static double Hash(bool value)
        {
            const double success = (1.0 + 7) * 13;
            const double failure = (-1.0 - 7) * 13;
            double result = value ? success : failure;
            return result;
        }
    }
}
