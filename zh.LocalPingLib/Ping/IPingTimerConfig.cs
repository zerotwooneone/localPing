using System;

namespace zh.LocalPingLib.Ping
{
    public interface IPingTimerConfig
    {
        TimeSpan IntervalBetweenPings { get; }
    }
}