using System;
using System.Collections.Generic;

namespace zh.LocalPingLib.Ping
{
    public class PingStats : IPingStats
    {
        public IList<bool> StatusHistory { get; set; }
        public double Average25 { get; set; }
        public int Average25Count { get; set; }
        public DateTime? LastFailure { get; set; }
        public DateTime? LastSuccess { get; set; }

        IEnumerable<bool> IPingStats.StatusHistory => StatusHistory;
        public PingStats(DateTime? lastSuccess, DateTime? lastFailure)
        {
            StatusHistory = new List<bool>();
            LastSuccess = lastSuccess;
            LastFailure = lastFailure;
        }

        
    }
}