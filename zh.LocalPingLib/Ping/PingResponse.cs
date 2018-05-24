using System;
using System.Net;
using System.Net.NetworkInformation;

namespace zh.LocalPingLib.Ping
{
    public class PingResponse : IPingResponse
    {
        public PingResponse(IPAddress ipAddress, TimeSpan roundTripTime, IPStatus status, IPAddress targetIpAddress)
        {
            ReponseIpAddress = ipAddress;
            RoundTripTime = roundTripTime;
            Status = status;
            TargetIpAddress = targetIpAddress;
        }

        public IPAddress TargetIpAddress { get; }
        public IPAddress ReponseIpAddress { get; }
        public TimeSpan RoundTripTime { get; }
        public IPStatus Status { get; }
    }
}