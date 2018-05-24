using System;
using System.Reactive.Linq;

namespace Desktop.Ping
{
    public class PingTimer : IPingTimer
    {
        private readonly IPingTimerConfig _pingTimerConfig;

        public PingTimer(IPingTimerConfig pingTimerConfig)
        {
            _pingTimerConfig = pingTimerConfig;
        }

        public IObservable<long> Start(Func<bool> stopCallback)
        {
            var observable = Observable.Interval(_pingTimerConfig.IntervalBetweenPings).Select(l=>
            {
                return l;
            });
            return observable;
        }
    }
}