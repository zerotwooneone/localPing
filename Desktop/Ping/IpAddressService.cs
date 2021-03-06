﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class IpAddressService : IIpAddressService
    {
        public IpAddressService()
        {
            var ipAddresses = Enumerable.Range(1, 255).Select(n => GetAddress(n)); //new[] { IPAddress.Parse("192.168.1.89") };
            IpAddressObservable = new BehaviorSubject<IEnumerable<IPAddress>>(ipAddresses);
        }

        public IObservable<IEnumerable<IPAddress>> IpAddressObservable { get; }

        

        public IPAddress GetAddress(int index)
        {
            return IPAddress.Parse($"192.168.1.{index}");
        }
    }
}