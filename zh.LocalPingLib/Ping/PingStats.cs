using System.Collections.Generic;

namespace zh.LocalPingLib.Ping
{
    public class PingStats : IPingStats
    {
        public IList<bool> StatusHistory { get; set; }
        public double Average25 { get; set; }
        public int Average25Count { get; set; }

        IEnumerable<bool> IPingStats.StatusHistory => StatusHistory;
        public PingStats()
        {
            StatusHistory = new List<bool>();
        }
    }
}