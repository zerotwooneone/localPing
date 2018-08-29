using System;
using System.Collections.Generic;
using System.Linq;
using zh.Vector;

namespace zh.LocalPingLib.Ping
{
    public class PingVectorFactory : IPingVectorFactory
    {
        private readonly IDimensionKeyFactory _dimensionKeyFactory;
        
        public PingVectorFactory(IDimensionKeyFactory dimensionKeyFactory)
        {
            _dimensionKeyFactory = dimensionKeyFactory;
        }
        public IVector GetVector(IPingResponse pingResponse, IPingStats stats)
        {
            IEnumerable<IDimensionValue> pingResponseValueX = GetPingResponseValuesX(pingResponse, r => GetStatsValue(r, stats)); //GetAddressDimensions,, pr => GetRttDimension(pr.RoundTripTime)
            IEnumerable<IDimensionValue> dimensionValues = pingResponseValueX;
            return new Vector.Vector(dimensionValues);
        }

        private IEnumerable<IDimensionValue> GetStatsValue(IPingResponse pingResponse, IPingStats stats)
        {
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
                last5Value = 0.0; 
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
            double allSuccessOrFailure = Hash(successCount == successes.Length || failureCount == successes.Length);
            double has1SuccessOrFailure = Hash(successCount == 1 || failureCount == 1);

            return new[]
            {
                new DimensionValue(_dimensionKeyFactory.GetOrCreate(DimensionNames.Is100PercentSuccessOrFailure), allSuccessOrFailure),
                new DimensionValue(_dimensionKeyFactory.GetOrCreate(DimensionNames.Has1SuccessOrFailure), has1SuccessOrFailure),
            };
        }

        private double Hash(DateTime timeStamp)
        {
            TimeSpan diff = DateTime.Now - timeStamp;
            long longValue = diff.Ticks;
            double doubleValue = (double)longValue;
            return Hash(doubleValue);
        }

        private IEnumerable<IDimensionValue> GetPingResponseValuesX(IPingResponse pingResponse,
            params Func<IPingResponse, IEnumerable<IDimensionValue>>[] valuesFuncs)
        {
            return valuesFuncs.Select(f => f(pingResponse)).SelectMany(f => f);
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
