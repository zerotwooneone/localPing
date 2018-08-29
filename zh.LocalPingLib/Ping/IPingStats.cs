using System;
using System.Collections.Generic;

namespace zh.LocalPingLib.Ping
{
    public interface IPingStats
    {
        IEnumerable<bool> StatusHistory { get; }
        double Average25 { get; }
        DateTime? LastFailure { get; }
        DateTime? LastSuccess { get; }
    }
}