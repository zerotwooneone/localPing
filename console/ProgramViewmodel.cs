using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace console
{
    public class ProgramViewmodel
    {
        private readonly ISubject<string> _messageSubject;
        public IObservable<string> MessageObservable => _messageSubject;
        public ProgramViewmodel()
        {
            _messageSubject = new Subject<string>();
        }
        public void Configure()
        {
            //nothing at the moment
        }

        public async Task Start()
        {
            var pings = Enumerable.Range(1, 255).Select(GetTask);
            var logs = pings.Select(t => t.ContinueWith(t1 => Log(t1.Result)));
            await Task.WhenAll(logs);
        }

        private void Log(PingCompletedEventArgs pingCompletedEventArgs)
        {
            Log(pingCompletedEventArgs.Reply.Address, pingCompletedEventArgs.Reply.RoundtripTime, pingCompletedEventArgs.Reply.Status);
        }

        private void Log(IPAddress address, long replyRoundtripTime, IPStatus replyStatus)
        {
            Log($"{DateTime.Now.ToShortTimeString()} Address:{address} RTT:{replyRoundtripTime} Status:{replyStatus}");
        }

        private void Log(string message)
        {
            _messageSubject.OnNext(message);
        }

        private Task<PingCompletedEventArgs> GetTask(int index)
        {
            var ipadrress = GetAddress(index);
            var ping = new Ping();
            var taskCompletionSource = new TaskCompletionSource<PingCompletedEventArgs>();

            void OnPingOnPingCompleted(object s, PingCompletedEventArgs e)
            {
                taskCompletionSource.SetResult(e);
            }

            ping.PingCompleted += OnPingOnPingCompleted;
            const int timeoutInMilliseconds = 1000;
            ping.SendAsync(ipadrress, timeoutInMilliseconds);
            return taskCompletionSource.Task;
        }

        private IPAddress GetAddress(int index)
        {
            return IPAddress.Parse($"192.168.1.{index}");
        }
    }
}