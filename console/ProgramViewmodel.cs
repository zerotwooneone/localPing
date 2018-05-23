using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using zh.LocalPing;

namespace console
{
    public class ProgramViewmodel
    {
        private readonly IPingService _pingService;
        private readonly ISubject<string> _messageSubject;
        public IObservable<string> MessageObservable => _messageSubject;

        public ProgramViewmodel(IPingService pingService)
        {
            _pingService = pingService;
            _messageSubject = new Subject<string>();
        }
        public void Configure()
        {
            //nothing at the moment
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            var ipAddresses = Enumerable.Range(1, 255).Select(n => GetAddress(n));
            var pings = _pingService.Ping(ipAddresses);
            var logs = pings.Select(t => t.ContinueWith(t1 => Log(t1.Result), cancellationToken));
            await Task.WhenAll(logs);
        }

        private void Log(IPingResponse pingResponse)
        {
            Log(pingResponse.IpAddress, pingResponse.RoundTripTime.TotalMilliseconds, pingResponse.Status);
        }

        private void Log(IPAddress address, double replyRoundtripTime, IPStatus replyStatus)
        {
            Log($"{DateTime.Now.ToShortTimeString()} Address:{address} RTT:{replyRoundtripTime} Status:{replyStatus}");
        }

        private void Log(string message)
        {
            _messageSubject.OnNext(message);
        }

        private IPAddress GetAddress(int index)
        {
            return IPAddress.Parse($"192.168.1.{index}");
        }
    }
}