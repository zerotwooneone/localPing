using System;
using System.Net;
using System.Net.NetworkInformation;

namespace zh.LocalPingLib.Ping
{
    public class PingResponseUtil : IPingResponseUtil
    {
        public IPingResponse Convert(PingCompletedEventArgs pingCompletedEventArgs)
        {
            var targetIpAddress = (IPAddress) pingCompletedEventArgs.UserState;
            return new PingResponse(pingCompletedEventArgs.Reply.Address, TimeSpan.FromMilliseconds(pingCompletedEventArgs.Reply.RoundtripTime), pingCompletedEventArgs.Reply.Status, targetIpAddress);
        }
    }
}