using System;

namespace zh.LocalPingLib.Ping
{
    public class Config : IPingTimerConfig
    {
        public TimeSpan IntervalBetweenPings => TimeSpan.FromSeconds(2);
    }
}