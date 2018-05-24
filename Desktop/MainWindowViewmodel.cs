using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Desktop.Ping;
using zh.LocalPingLib.Ping;

namespace Desktop
{
    public class MainWindowViewmodel
    {
        private readonly IPingTimer _pingTimer;
        private readonly IPingService _pingService;

        public MainWindowViewmodel(IPingTimer pingTimer, IPingService pingService)
        {
            _pingTimer = pingTimer;
            _pingService = pingService;
            IObservable<long> o = _pingTimer.Start(() => false);
            var ipAddresses = Enumerable.Range(1, 255).Select(n => GetAddress(n));
            IObservable<IEnumerable<Task<IPingResponse>>> x = o.Select(l => _pingService.Ping(ipAddresses));
            x.Subscribe(async a =>
            {
                var tasks= a.Select(async y =>
                {
                    var z = await y;
                    Debug.WriteLine($"address:{z.IpAddress} RTT:{z.RoundTripTime.TotalMilliseconds} status:{z.Status}");
                });
                await Task.WhenAll(tasks);
            });
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }

        private IPAddress GetAddress(int index)
        {
            return IPAddress.Parse($"192.168.1.{index}");
        }
    }
}