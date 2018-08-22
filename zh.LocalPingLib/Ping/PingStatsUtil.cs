using System;
using System.Collections.Generic;
using System.Linq;

namespace zh.LocalPingLib.Ping
{
    public class PingStatsUtil
    {
        private const int MaxHistoryCount = 55;

        public void AddStatus(IList<bool> statusHistory, bool status)
        {
            statusHistory.Add(status);
            if (statusHistory.Count > MaxHistoryCount)
            {
                statusHistory.RemoveAt(0);
            }
        }

        public Stats GetAverageSuccessRate(IEnumerable<bool> statusHistory)
        {
            var vals = statusHistory.Select(b => b ? 1.0 : 0).ToArray();
            var aggregate = vals.Aggregate(new Aggregates(), (s, val) =>
            {
                if (!s.Min.HasValue || val < s.Min)
                {
                    s.Min = val;
                }
                if (!s.Max.HasValue || val > s.Max)
                {
                    s.Max = val;
                }

                s.Sum += val;
                s.Count++;
                return s;
            });
            var average = aggregate.Sum / aggregate.Count;
            Stats stats=new Stats { Average = average };
            const double tollerance = 0.001;
            if (Math.Abs((aggregate.Min??0) - (aggregate.Max??0)) < tollerance)
            {
                stats.StdDev = 0;
                stats.StdDevMinMax = 0;
            }
            else
            {
                stats = vals.Aggregate(stats, (s, val) =>
                {
                
                    bool isMinMax = false;
                    if (aggregate.Min.HasValue && Math.Abs(aggregate.Min.Value - val) < tollerance)
                    {
                        aggregate.Min = null;
                        isMinMax = true;
                    }
                    if (aggregate.Max.HasValue && Math.Abs(aggregate.Max.Value - val) < tollerance)
                    {
                        aggregate.Max = null;
                        isMinMax = true;
                    }

                    var diff = val - average;
                    var diffSquared = Math.Pow(diff, 2);
                    s.StdDev += diffSquared;
                    if (!isMinMax)
                    {
                        s.StdDevMinMax += diffSquared;
                    }

                    return s;
                });

                var stdDevFrac = stats.StdDev / aggregate.Count;
                var stdDev = Math.Sqrt(stdDevFrac);
                stats.StdDev = stdDev;

                var stdDevMMFrac = stats.StdDevMinMax / (aggregate.Count-2);
                var stdDevMM = Math.Sqrt(stdDevMMFrac);
                stats.StdDevMinMax = stdDevMM;
            }

            return stats;
        }
    }
}