using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Desktop.Ping;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop
{
    public class MainWindowViewmodel
    {
        private readonly IPingTimer _pingTimer;
        private readonly IPingService _pingService;
        private readonly IPingCollectionVectorFactory _pingCollectionVectorFactory;
        private readonly IVectorComparer _vectorComparer;
        private IVector _previousVector;

        public MainWindowViewmodel(IPingTimer pingTimer, 
            IPingService pingService, 
            IPingCollectionVectorFactory pingCollectionVectorFactory,
            IVectorComparer vectorComparer)
        {
            _pingTimer = pingTimer;
            _pingService = pingService;
            _pingCollectionVectorFactory = pingCollectionVectorFactory;
            _vectorComparer = vectorComparer;
            IObservable<long> o = _pingTimer.Start(() => false);
            var ipAddresses = Enumerable.Range(1, 255).Select(n => GetAddress(n)); //new[] {IPAddress.Parse("192.168.1.146")};
            IObservable<IEnumerable<Task<IPingResponse>>> x = o.Select(l => _pingService.Ping(ipAddresses));
            x.Subscribe(async a =>
            {
                var tasks= a.Select(async y =>
                {
                    var z = await y;
                    //Log($"address:{z.ReponseIpAddress} RTT:{z.RoundTripTime.TotalMilliseconds} status:{z.Status}");
                    return z;
                });
                var responses = await Task.WhenAll(tasks);
                var currentVector = _pingCollectionVectorFactory.GeVector(responses);
                if (_previousVector != null)
                {
                    Log($"diff:{_vectorComparer.Compare(_previousVector, currentVector)}");
                }
                _previousVector = currentVector;
            });
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }

        private IPAddress GetAddress(int index)
        {
            return IPAddress.Parse($"192.168.1.{index}");
        }
    }
}