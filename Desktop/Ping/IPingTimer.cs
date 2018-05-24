using System;

namespace Desktop.Ping
{
    public interface IPingTimer
    {
        IObservable<long> Start(Func<bool> stopCallback);
    }
}