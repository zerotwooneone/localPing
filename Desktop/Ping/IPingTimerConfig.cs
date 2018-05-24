using System;

namespace Desktop.Ping
{
    public interface IPingTimerConfig
    {
        TimeSpan IntervalBetweenPings { get; }
    }
}