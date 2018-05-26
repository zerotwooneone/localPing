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
            IVectorComparer vectorComparer,
            IIpAddressService ipAddressService)
        {
            _pingTimer = pingTimer;
            _pingService = pingService;
            _pingCollectionVectorFactory = pingCollectionVectorFactory;
            _vectorComparer = vectorComparer;
            IObservable<long> pingTimerObservable = _pingTimer.Start(() => false);
            IDisposable pingResponseSubscription = null;

            ipAddressService.IpAddressObservable.Subscribe(ipAddresses =>
            {
                pingResponseSubscription?.Dispose();
                IObservable<IEnumerable<Task<IPingResponse>>> pingResponseObservable =
                    pingTimerObservable.Select(l => _pingService.Ping(ipAddresses));
                pingResponseSubscription = pingResponseObservable.Subscribe(async pingResponseTasks =>
                {
                    var tasks = pingResponseTasks.Select(async pingResponseTask =>
                    {
                        var pingResponse = await pingResponseTask;
                        //Log($"address:{pingResponse.ReponseIpAddress} RTT:{pingResponse.RoundTripTime.TotalMilliseconds} status:{pingResponse.Status}");
                        return pingResponse;
                    });
                    var responses = await Task.WhenAll(tasks);
                    var currentVector = await _pingCollectionVectorFactory.GeVector(responses);
                    if (_previousVector != null)
                    {
                        Log($"diff:{_vectorComparer.Compare(_previousVector, currentVector)}");
                    }

                    _previousVector = currentVector;
                });
            });


        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }

        
    }
}