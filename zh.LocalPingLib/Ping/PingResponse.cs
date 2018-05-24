using System;
using System.Net;
using System.Net.NetworkInformation;

namespace zh.LocalPingLib.Ping
{
    public class PingResponse : IPingResponse
    {
        public PingResponse(IPAddress ipAddress, TimeSpan roundTripTime, IPStatus status)
        {
            IpAddress = ipAddress;
            RoundTripTime = roundTripTime;
            Status = status;
        }

        public IPAddress IpAddress { get; }
        public TimeSpan RoundTripTime { get; }
        public IPStatus Status { get; }
    }
}