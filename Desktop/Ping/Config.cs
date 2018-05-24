using System;

namespace Desktop.Ping
{
    public class Config : IPingTimerConfig
    {
        public TimeSpan IntervalBetweenPings => TimeSpan.FromSeconds(2);
    }
}