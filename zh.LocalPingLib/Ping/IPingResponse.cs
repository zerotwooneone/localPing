using System;
using System.Net;
using System.Net.NetworkInformation;

namespace zh.LocalPingLib.Ping
{
    public interface IPingResponse
    {
        IPAddress IpAddress { get; }
        TimeSpan RoundTripTime { get; }
        IPStatus Status { get; }
    }
}