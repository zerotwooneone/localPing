using System;
using System.Net;
using System.Net.NetworkInformation;

namespace zh.LocalPing
{
    public interface IPingResponse
    {
        IPAddress IpAddress { get; }
        TimeSpan RoundTripTime { get; }
        IPStatus Status { get; }
    }
}