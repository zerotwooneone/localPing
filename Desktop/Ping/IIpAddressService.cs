using System;
using System.Collections.Generic;
using System.Net;

namespace Desktop.Ping
{
    public interface IIpAddressService
    {
        IObservable<IEnumerable<IPAddress>> IpAddressObservable { get; }
    }
}