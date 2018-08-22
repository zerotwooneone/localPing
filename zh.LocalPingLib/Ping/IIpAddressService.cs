using System;
using System.Collections.Generic;
using System.Net;

namespace zh.LocalPingLib.Ping
{
    public interface IIpAddressService
    {
        IObservable<IEnumerable<IPAddress>> IpAddressObservable { get; }
    }
}