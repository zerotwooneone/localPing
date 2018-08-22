using Desktop.Vector;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return GetVectorInternal(pingResponse, stats, _pingResponseUtil, _dimensionKeyFactory, _pingStatsUtil);
        }

        public static IVector GetVectorInternal(IPingResponse pingResponse, IPingStats stats, IPingResponseUtil pingResponseUtil, IDimensionKeyFactory dimensionKeyFactory, PingStatsUtil pingStatsUtil)
        {
            IDimensionValue ValuesFunc(IPingResponse pr) => GetStatusDimensionInternal(pingResponse, pingResponseUtil, dimensionKeyFactory);

            IEnumerable<IDimensionValue> pingResponseValues = GetPingResponseValuesInternal(pingResponse, ValuesFunc);

            IEnumerable<IDimensionValue> AddressDimensions(IPingResponse pr) => GetAddressDimensionsInternal(pr, dimensionKeyFactory);


            IEnumerable<IDimensionValue> StatsDimesions(IPingResponse r) => GetStatsValue(r, stats, pingStatsUtil, pingResponseUtil, dimensionKeyFactory);

            IEnumerable<IDimensionValue> RttDimensioons(IPingResponse pr) => GetRttDimension(pr.RoundTripTime, dimensionKeyFactory);

            IEnumerable<IDimensionValue> pingResponseValueX = GetPingResponseValuesX(pingResponse, AddressDimensions, StatsDimesions, RttDimensioons);
            IEnumerable<IDimensionValue> dimensionValues = pingResponseValues.Concat(pingResponseValueX);
            return new Desktop.Vector.Vector(dimensionValues);
        }

        public static IEnumerable<IDimensionValue> GetRttDimension(TimeSpan roundTripTime, IDimensionKeyFactory dimensionKeyFactory)
        {
            double rtt = Hash(roundTripTime.TotalMilliseconds);
            IDimensionKey rttValueDimensionName = dimensionKeyFactory.GetOrCreate($"Round Trip Time {roundTripTime.ToString()}");
            return new[]
            {
                new DimensionValue(rttValueDimensionName, rtt),
            };
        }

        public static IEnumerable<IDimensionValue> GetStatsValue(IPingResponse pingResponse, IPingStats stats, PingStatsUtil pingStatsUtil, IPingResponseUtil pingResponseUtil, IDimensionKeyFactory dimensionKeyFactory)
        {
            Stats statsObj = pingStatsUtil.GetAverageSuccessRate(stats.StatusHistory);

            bool success = pingResponseUtil.IsSuccess(pingResponse.Status);
            double successVal = success ? 1.0 : 0;

            bool aboveAverage = successVal > statsObj.Average;
            DimensionValue greaterThanAverage = new DimensionValue(dimensionKeyFactory.GetOrCreate("Above Average"), aboveAverage ? Hash(true) : 0);
            //var lessThanAverage = new DimensionValue(_dimensionKeyFactory.GetOrCreate("Below Average"), !aboveAverage ? Hash(true):0);

            double halfStdDev = statsObj.StdDev / 2;
            double avgPlusHalf = statsObj.Average + halfStdDev;
            bool aboveAvgPlusHalf = successVal > avgPlusHalf;

            DimensionValue greaterThanAveragePlusHalf = new DimensionValue(dimensionKeyFactory.GetOrCreate("Above Average Plus Half Std Dev"), aboveAvgPlusHalf ? Hash(true) : 0);
            //var lessThanAveragePlusHalf = new DimensionValue(_dimensionKeyFactory.GetOrCreate("Below Average Plus Half Std Dev"), !aboveAvgPlusHalf ? Hash(true):0);

            double avgMinusHalf = statsObj.Average - halfStdDev;
            bool aboveAvgMinusHalf = successVal > avgMinusHalf;

            DimensionValue greaterThanAverageMinusHalf = new DimensionValue(dimensionKeyFactory.GetOrCreate("Above Average Minus Half Std Dev"), aboveAvgMinusHalf ? Hash(true) : 0);
            //var lessThanAverageMinusHalf = new DimensionValue(_dimensionKeyFactory.GetOrCreate("Below Average Minus Half Std Dev"), !aboveAvgMinusHalf ? Hash(true):0);

            //var averageSuccessRate = Hash(statsObj.Average);
            //var scopedAverageDimensionName = _dimensionKeyFactory.GetOrCreate($"Average Success Rate {averageSuccessRate}");
            //var avg25ValueName = _dimensionKeyFactory.GetOrCreate($"Average 25 {stats.Average25}");
            //var avg25Value = Hash(stats.Average25) * 1000;
            IEnumerable<bool?> nullableStatus = stats.StatusHistory.Cast<bool?>();
            IEnumerable<IDimensionValue> statusHistoryDimensionValues = GetStatusHistoryDimensionValues(nullableStatus, dimensionKeyFactory);
            return new[] {
                //new DimensionValue(scopedAverageDimensionName, averageSuccessRate),
                //new DimensionValue(avg25ValueName, avg25Value)
                greaterThanAverage,
                //lessThanAverage,
                greaterThanAverageMinusHalf,
                greaterThanAveragePlusHalf,
                //lessThanAverageMinusHalf,
                //lessThanAveragePlusHalf
            }.Concat(statusHistoryDimensionValues);
        }

        public static IEnumerable<IDimensionValue> GetStatusHistoryDimensionValues(IEnumerable<bool?> nullableStatus, IDimensionKeyFactory dimensionKeyFactory)
        {
            IEnumerable<bool> statusSuccesses = nullableStatus
                .Select(nb => nb ?? true);
            bool[] successes = statusSuccesses as bool[] ?? statusSuccesses.ToArray();
            int successCount = successes.Count(b => b);
            //var failureCount = successes.Count(b => !b);
            int successPct = successCount / successes.Length;
            int failurePct = 1 - successPct;
            successPct *= 100;
            failurePct *= 100;
            IEnumerable<DimensionValue> pctDims = Enumerable.Range(1, 9).Select(i =>
            {
                int pct = i * 10;
                bool isGreaterThanSuccessPct = successPct > pct;
                bool isGreaterThanFailurePct = failurePct > pct;
                return new[]
                {

                    new DimensionValue(dimensionKeyFactory.GetOrCreate($"Greater than {pct}% success"),
                        isGreaterThanSuccessPct ? Hash(true) : 0),
                    //new DimensionValue(_dimensionKeyFactory.GetOrCreate($"Less than {pct}% success"), !isGreaterThanPct ? Hash(true):0)
                    new DimensionValue(dimensionKeyFactory.GetOrCreate($"Greater than {pct}% failure"),
                        isGreaterThanFailurePct ? Hash(true) : 0),
                };
            }).SelectMany(i => i);
            //var successDims = Enumerable.Range(0, successCount).Select(i =>
            //{
            //    var n = _dimensionKeyFactory.GetOrCreate($"Has {i} successes");
            //    return new DimensionValue(n, Hash(i));
            //});
            //var failureDims = Enumerable.Range(0, failureCount).Select(i =>
            //{
            //    var n = _dimensionKeyFactory.GetOrCreate($"Has {i} failures");
            //    return new DimensionValue(n, Hash(i));
            //});
            return successes
                .SelectMany((b, i) => StatusHistoryDimensionValues(i, b, dimensionKeyFactory))
                //.Concat(
                //    new[]
                //    {
                //        new DimensionValue(_dimensionKeyFactory.GetOrCreate($"Success Count {successCount}"), Hash(successCount)),
                //        new DimensionValue(_dimensionKeyFactory.GetOrCreate($"Failure Count {failureCount}"), Hash(failureCount)),
                //    }
                //    )
                //.Concat(successDims)
                //.Concat(failureDims)
                .Concat(pctDims);
        }

        public static IEnumerable<IDimensionValue> StatusHistoryDimensionValues(int index, bool lastStatus, IDimensionKeyFactory dimensionKeyFactory)
        {
            double lastSuccess = Hash(lastStatus);
            IDimensionKey lastSuccessDimensionName = dimensionKeyFactory.GetOrCreate($"Last Success {index} {lastSuccess}");
            DimensionValue lsDimensionValue = new DimensionValue(lastSuccessDimensionName, Hash(lastSuccess));
            double lastFailure = Hash(!lastStatus);
            IDimensionKey lastFailureDimensionName = dimensionKeyFactory.GetOrCreate($"Last Failure {index} {lastFailure}");
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

        public IEnumerable<IDimensionValue> GetPingResponseValues(IPingResponse pingResponse,
            params Func<IPingResponse, IDimensionValue>[] valuesFuncs)
        {
            return GetPingResponseValuesInternal(pingResponse, valuesFuncs);
        }

        public static IEnumerable<IDimensionValue> GetPingResponseValuesInternal(IPingResponse pingResponse,
            params Func<IPingResponse, IDimensionValue>[] valuesFuncs)
        {
            return valuesFuncs.Select(f => f(pingResponse));
        }

        public static IEnumerable<IDimensionValue> GetPingResponseValuesX(IPingResponse pingResponse,
            params Func<IPingResponse, IEnumerable<IDimensionValue>>[] valuesFuncs)
        {
            return valuesFuncs.Select(f => f(pingResponse)).SelectMany(f => f);
        }

        public static DimensionValue GetStatusDimensionInternal(IPingResponse pingResponse, IPingResponseUtil pingResponseUtil, IDimensionKeyFactory dimensionKeyFactory)
        {
            bool isSuccess = pingResponseUtil.IsSuccess(pingResponse.Status);
            double statusFlagValue = Hash(isSuccess);
            IDimensionKey dimensionKey = dimensionKeyFactory.GetOrCreate($"status flag {isSuccess}");
            DimensionValue statusDimensionValue = new DimensionValue(dimensionKey, statusFlagValue);
            return statusDimensionValue;
        }

        public static IEnumerable<IDimensionValue> GetAddressDimensionsInternal(IPingResponse pingResponse, IDimensionKeyFactory dimensionKeyFactory)
        {
            int intValue = pingResponse.TargetIpAddress.GetHashCode();
            double ipValue = Hash(intValue);

            IDimensionKey ipDimensionKey = dimensionKeyFactory.GetOrCreate("Ip Address");
            DimensionValue ipSpecDimensionValue = new DimensionValue(ipDimensionKey, ipValue);

            return new[]
            {
                ipSpecDimensionValue
            };
        }
        private const double Modulus = 1000;
        public static double Hash(int value)
        {
            return ((value + 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }

        public static double Hash(double value)
        {
            return ((value + 7.0) * 13) % Modulus; //add and mult by prime numbers to avoid zero values and to space out consecutive integers
        }

        public static double Hash(bool value)
        {
            const double success = (1.0 + 7) * 13;
            const double failure = (0.0 + 7) * 13;
            double result = value ? success : failure;
            return result;
        }
    }
}
