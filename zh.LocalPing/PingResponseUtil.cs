using System;
using System.Net.NetworkInformation;

namespace zh.LocalPing
{
    public class PingResponseUtil : IPingResponseUtil
    {
        public IPingResponse Convert(PingCompletedEventArgs pingCompletedEventArgs)
        {
            return new PingResponse(pingCompletedEventArgs.Reply.Address, TimeSpan.FromMilliseconds(pingCompletedEventArgs.Reply.RoundtripTime), pingCompletedEventArgs.Reply.Status);
        }
    }
}