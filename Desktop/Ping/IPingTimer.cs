using System;
using System.Reactive.Subjects;

namespace Desktop.Ping
{
    public interface IPingTimer
    {
        IObservable<long> Start(Func<bool> stopCallback);
    }
}