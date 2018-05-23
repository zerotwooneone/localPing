﻿using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace zh.LocalPing
{
    public class PingTaskFactory : IPingTaskFactory
    {
        private readonly IPingResponseUtil _pingResponseUtil;

        public PingTaskFactory(IPingResponseUtil pingResponseUtil)
        {
            _pingResponseUtil = pingResponseUtil;
        }

        public async Task<IPingResponse> Ping(IPAddress ipadrress)
        {
            var ping = new Ping();
            var taskCompletionSource = new TaskCompletionSource<PingCompletedEventArgs>();

            void OnPingOnPingCompleted(object s, PingCompletedEventArgs e)
            {
                taskCompletionSource.SetResult(e);
            }

            ping.PingCompleted += OnPingOnPingCompleted;
            const int timeoutInMilliseconds = 1000;
            ping.SendAsync(ipadrress, timeoutInMilliseconds);

            var responsEventArgs = await taskCompletionSource.Task;
            var result = _pingResponseUtil.Convert(responsEventArgs);
            return result;
        }
    }
}