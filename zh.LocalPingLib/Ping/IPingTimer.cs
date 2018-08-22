using System;

namespace zh.LocalPingLib.Ping
{
    public interface IPingTimer
    {
        IObservable<long> Start(Func<bool> stopCallback);
    }
}